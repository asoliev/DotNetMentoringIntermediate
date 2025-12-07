using Azure.Messaging.ServiceBus;
using DocStream.Shared;
using System.Diagnostics;
using System.Text.Json;

namespace DocStream.CaptureService.Producer;

internal class Producer
{
    private const int FileLockCheckIntervalMs = 500;
    private readonly TimeSpan FileLockTimeout = TimeSpan.FromSeconds(30); // adjust as needed
    private readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(5);

    public void Start()
    {
        using var watcher = new FileSystemWatcher(AppConfig.InputFolder, "*.pdf");

        watcher.Changed += (s, e) => { Utils.PrintWithTimestamp("Watcher.Changed"); };
        watcher.Created += OnFileCreated;
        watcher.EnableRaisingEvents = true;

        Utils.PrintWithTimestamp("Watching for new files...");
        Console.ReadLine();
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e) => _ = HandleFileCreatedAsync(e); // Fire-and-forget the async worker; the worker itself handles exceptions.

    private async Task HandleFileCreatedAsync(FileSystemEventArgs e)
    {
        try
        {
            Utils.PrintWithTimestamp($"Detected new file: {e.FullPath}");

            // Wait for the file to be fully written, but enforce a timeout to avoid infinite waits.
            if (!await WaitForFileReadyAsync(e.FullPath))
                return;

            // Double-check file still exists before proceeding
            if (!File.Exists(e.FullPath))
            {
                Utils.PrintWithTimestamp($"File disappeared before processing: {e.FullPath}");
                return;
            }

            // Now process the file
            var chunks = SplitFile(e.FullPath, AppConfig.ChunkSize).ToList();
            int totalChunks = chunks.Count;

            var fileId = Guid.NewGuid().ToString();
            string fileHash = Utils.ComputeSHA256(e.FullPath);

            await SendChunksAsync(fileId, e.Name, fileHash, chunks, totalChunks);

            Utils.PrintWithTimestamp($"Sent {totalChunks} chunks for {e.Name} (FileId: {fileId})");
        }
        catch (Exception ex)
        {
            // Centralized exception handling (prevents unobserved exceptions)
            Utils.PrintWithTimestamp($"Error sending file '{e.FullPath}': {ex.Message}");
        }
    }

    private async Task<bool> WaitForFileReadyAsync(string filePath)
    {
        var sw = Stopwatch.StartNew();
        while (IsFileLocked(filePath))
        {
            if (sw.Elapsed > FileLockTimeout)
            {
                Utils.PrintWithTimestamp($"Timeout waiting for file to be released: {filePath} after {FileLockTimeout.TotalSeconds:F0}s. Aborting processing for this file.");
                return false; // Abort processing to avoid infinite loop
            }

            // If the file was deleted or moved while waiting, stop trying.
            if (!File.Exists(filePath))
            {
                Utils.PrintWithTimestamp($"File no longer exists while waiting: {filePath}. Aborting.");
                return false;
            }

            await Task.Delay(FileLockCheckIntervalMs);
        }

        return true;
    }

    private async Task SendChunksAsync(string fileId, string? fileName, string fileHash, List<byte[]> chunks, int totalChunks)
    {
        // Set the transport type to AmqpWebSockets so that the ServiceBusClient uses the port 443.
        // If you use the default AmqpTcp, ensure that ports 5671 and 5672 are open.
        var clientOptions = new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets };

        await using var client = new ServiceBusClient(AppConfig.ServiceBusConnectionString, clientOptions);
        await using var sender = client.CreateSender(AppConfig.QueueName);

        for (int i = 0; i < totalChunks; i++)
        {
            var message = CreateMessageForChunk(fileId, fileName, fileHash, chunks[i], i, totalChunks);

            await SendMessageBatchWithTimeoutAsync(sender, message, SendTimeout);

            // Optional: show sending progress
            PrintProgressBar(i + 1, totalChunks, fileName);
        }
    }

    private ServiceBusMessage CreateMessageForChunk(string fileId, string? fileName, string fileHash, byte[] chunkData, int chunkNumber, int totalChunks)
    {
        object messageObj = (
            FileId: fileId,
            FileName: fileName,
            ChunkNumber: chunkNumber,
            TotalChunks: totalChunks,
            Data: Convert.ToBase64String(chunkData),
            FileHash: chunkNumber == 0 ? fileHash : null // Only first chunk carries the hash
        );

        string messageBody = JsonSerializer.Serialize(messageObj);
        ServiceBusMessage message = new(messageBody)
        {
            MessageId = $"{fileId}-{chunkNumber}",
            SessionId = fileId
        };

        return message;
    }

    private async Task SendMessageBatchWithTimeoutAsync(ServiceBusSender sender, ServiceBusMessage message, TimeSpan timeout)
    {
        using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
        if (!messageBatch.TryAddMessage(message))
            throw new Exception("Chunk is too large to fit in a single ServiceBusMessageBatch.");

        using var cts = new CancellationTokenSource(timeout);
        await sender.SendMessagesAsync(messageBatch, cts.Token);
    }

    private void PrintProgressBar(int sent, int total, string? fileName, int barSize = 30)
    {
        double percent = (double)sent / total;

        int filled = (int)(percent * barSize);

        int empty = barSize - filled;

        string bar = new string('█', filled) + new string('-', empty);

        string progress = $"Sending '{fileName}': [{bar}] {percent * 100:F1}% ({sent}/{total} chunks)";

        Console.Write($"\r{progress.PadRight(Console.WindowWidth - 1)}");

        if (sent == total)
            Console.WriteLine();
    }

    private IEnumerable<byte[]> SplitFile(string filePath, int chunkSize)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        byte[] buffer = new byte[chunkSize];
        int bytesRead;
        while ((bytesRead = fs.Read(buffer, 0, chunkSize)) > 0)
        {
            if (bytesRead < chunkSize)
                yield return buffer.Take(bytesRead).ToArray();
            else
                yield return buffer.ToArray();
        }
    }

    private bool IsFileLocked(string file)
    {
        try
        {
            using FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
    }
}