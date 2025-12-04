using Azure.Messaging.ServiceBus;
using DocStream.Shared;
using System.Text.Json;

namespace DocStream.CaptureService.Producer;

/// <summary> Producer </summary>
internal class Program
{
    private static ServiceBusSender senderInstance;

    private static async Task Main()
    {
        var client = new ServiceBusClient(AppConfig.ServiceBusConnectionString);
        senderInstance = client.CreateSender(AppConfig.QueueName);

        if (!Directory.Exists(AppConfig.InputFolder))
            Directory.CreateDirectory(AppConfig.InputFolder);

        var watcher = new FileSystemWatcher(AppConfig.InputFolder, "*.pdf");

        watcher.Changed += (s, e) => { PrintWithTimestamp("Watcher.Changed"); };
        watcher.Created += OnFileCreated;
        watcher.EnableRaisingEvents = true;

        PrintWithTimestamp("Watching for new files...");
        Console.ReadLine();

        watcher.Dispose();
        await senderInstance.DisposeAsync();
        await client.DisposeAsync();
    }

    private static async void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        try
        {
            PrintWithTimestamp($"Detected new file: {e.FullPath}");

            // Wait for the file to be fully written
            while (IsFileLocked(e.FullPath))
                await Task.Delay(500);

            // Now process the file
            var fileId = Guid.NewGuid().ToString();
            var chunks = SplitFile(e.FullPath, AppConfig.ChunkSize).ToList();
            string fileHash = Utils.ComputeSHA256(e.FullPath);
            int totalChunks = chunks.Count;

            await SendChunksAsync(fileId, e.FullPath, fileHash, chunks, totalChunks);

            PrintWithTimestamp($"Sent {totalChunks} chunks for {e.Name} (FileId: {fileId})");
        }
        catch (Exception ex)
        {
            PrintWithTimestamp($"Error sending file '{e.FullPath}': {ex.Message}");
        }
    }

    private static async Task SendChunksAsync(string fileId, string filePath, string fileHash, List<byte[]> chunks, int totalChunks)
    {
        for (int i = 0; i < totalChunks; i++)
        {
            var messageObj = new
            {
                FileId = fileId,
                FileName = Path.GetFileName(filePath),
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
            await senderInstance.SendMessageAsync(message);

            // Optional: show sending progress
            PrintProgressBar(i + 1, totalChunks, Path.GetFileName(filePath));
        }
    }

    static void PrintWithTimestamp(string message) => Console.WriteLine($"[{DateTime.Now:yyyy/MMM/dd-HH:mm:ss}] {message}");

    private static void PrintProgressBar(int sent, int total, string fileName, int barSize = 30)
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
        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
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