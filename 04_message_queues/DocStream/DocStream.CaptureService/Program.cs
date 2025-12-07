using Azure.Messaging.ServiceBus;
using DocStream.Shared;
using System.Text.Json;

namespace DocStream.CaptureService.Producer;

/// <summary> Producer </summary>
internal class Program
{
    private static async Task Main()
    {
        using var watcher = new FileSystemWatcher(AppConfig.InputFolder, "*.pdf");

        watcher.Changed += (s, e) => { Utils.PrintWithTimestamp("Watcher.Changed"); };
        watcher.Created += OnFileCreated;
        watcher.EnableRaisingEvents = true;

        Utils.PrintWithTimestamp("Watching for new files...");
        Console.ReadLine();
    }

    private static void OnFileCreated(object sender, FileSystemEventArgs e) => _ = HandleFileCreatedAsync(e);// Fire-and-forget the async worker; the worker itself handles exceptions.

    private static async Task HandleFileCreatedAsync(FileSystemEventArgs e)
    {
        try
        {
            Utils.PrintWithTimestamp($"Detected new file: {e.FullPath}");

            // Wait for the file to be fully written
            while (IsFileLocked(e.FullPath))
                await Task.Delay(500);

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

    private static async Task SendChunksAsync(string fileId, string? fileName, string fileHash, List<byte[]> chunks, int totalChunks)
    {
        // Set the transport type to AmqpWebSockets so that the ServiceBusClient uses the port 443.
        // If you use the default AmqpTcp, ensure that ports 5671 and 5672 are open.
        var clientOptions = new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets };

        await using var client = new ServiceBusClient(AppConfig.ServiceBusConnectionString, clientOptions);
        await using var sender = client.CreateSender(AppConfig.QueueName);

        //ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
        var messages = new List<ServiceBusMessage>();
        for (int i = 0; i < totalChunks; i++)
        {
            var messageObj = new
            {
                FileId = fileId,
                FileName = fileName,
                ChunkNumber = i,
                TotalChunks = totalChunks,
                Data = Convert.ToBase64String(chunks[i]),
                FileHash = i == 0 ? fileHash : null // Only first chunk carries the hash
            };
            var messageBody = JsonSerializer.Serialize(messageObj);
            var message = new ServiceBusMessage(messageBody)
            {
                MessageId = $"{fileId}-{i}",
                SessionId = fileId
            };
            messages.Add(message);

            // Create a new batch for each chunk
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
            if (!messageBatch.TryAddMessage(message))
                throw new Exception("Chunk is too large to fit in a single ServiceBusMessageBatch.");

            // Set a timeout (e.g., 5 seconds)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await sender.SendMessagesAsync(messageBatch);

            // Optional: show sending progress
            PrintProgressBar(i + 1, totalChunks, fileName);
        }
    }

    private static void PrintProgressBar(int sent, int total, string? fileName, int barSize = 30)
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

    private static IEnumerable<byte[]> SplitFile(string filePath, int chunkSize)
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

    private static bool IsFileLocked(string file)
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