using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace DocStream.CaptureService.Producer;

/// <summary> Producer </summary>
internal class Program
{
    private static ServiceBusSender senderInstance;
    private static int chunkSize;

    private static async Task Main()
    {
        string inputFolder = @"C:\Users\Abdukodir_Soliev\Downloads\MessageQueue\Input";
        string queueName = "filechunks";
        string serviceBusConnectionString = "Endpoint=sb://docprocessing-ns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=WUdorXDZVjCI4hVVvYHlWAl3V3vw03KrY+ASbPHWhTQ=";

        // Adjust chunk size to account for Base64 encoding overhead
        // The message size includes the base64-encoded chunk, plus JSON and headers.
        // Base64 encoding increases size by ~33 %.For example, a 220 KB binary chunk becomes ~293 KB when base64 - encoded.
        // The JSON envelope adds more bytes.
        chunkSize = 170 * 1024; //For 256 KB: 262,144 / 1.37 ≈ 191,400 bytes (about 187 KB)

        var client = new ServiceBusClient(serviceBusConnectionString);
        senderInstance = client.CreateSender(queueName);

        if (!Directory.Exists(inputFolder))
            Directory.CreateDirectory(inputFolder);

        var watcher = new FileSystemWatcher(inputFolder, "*.pdf");
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
            var chunks = SplitFile(e.FullPath, chunkSize).ToList();
            int totalChunks = chunks.Count;
            string fileHash = ComputeSHA256(e.FullPath);

            for (int i = 0; i < totalChunks; i++)
            {
                var messageObj = new
                {
                    FileId = fileId,
                    FileName = Path.GetFileName(e.FullPath),
                    ChunkNumber = i,
                    TotalChunks = totalChunks,
                    Data = Convert.ToBase64String(chunks[i]),
                    FileHash = i == 0 ? fileHash : null // Only first chunk carries the hash
                };
                var messageBody = JsonSerializer.Serialize(messageObj);
                var message = new ServiceBusMessage(messageBody)
                {
                    MessageId = $"{fileId}-{i}",
                    SessionId = fileId // Ensures all chunks for a file are grouped
                };
                await senderInstance.SendMessageAsync(message);

                // Optional: show sending progress
                PrintProgressBar(i + 1, totalChunks, Path.GetFileName(e.FullPath));
            }

            PrintWithTimestamp($"Sent {totalChunks} chunks for {e.Name} (FileId: {fileId})");
        }
        catch (Exception ex)
        {
            PrintWithTimestamp($"Error sending file '{e.FullPath}': {ex.Message}");
        }
    };

    static void PrintWithTimestamp(string message)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy/MMM/dd-HH:mm:ss}] {message}");
    }
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
    private static string ComputeSHA256(string filePath)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return Convert.ToHexStringLower(hash);//Convert.ToHexStringLower(hash);
    }
    private static bool IsFileLocked(string file)
    {
        try
        {
            using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None))
                return false;
        }
        catch (IOException)
        {
            return true;
        }
    }
}