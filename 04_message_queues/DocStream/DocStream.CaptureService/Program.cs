using Azure.Messaging.ServiceBus;
using System.Text.Json;

internal class Program
{
    private static async Task Main(string[] args)
    {
        string inputFolder = @"C:\Input";
        string queueName = "file_chunks";
        string serviceBusConnectionString = "Endpoint=sb://docprocessing-ns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=WUdorXDZVjCI4hVVvYHlWAl3V3vw03KrY+ASbPHWhTQ=";
        int chunkSize = 1024 * 1024 * 4; // 4MB

        var client = new ServiceBusClient(serviceBusConnectionString);
        var sender = client.CreateSender(queueName);

        var watcher = new FileSystemWatcher(inputFolder, "*.pdf");
        watcher.Created += async (s, e) =>
        {
            Console.WriteLine($"Detected new file: {e.FullPath}");
            var fileId = Guid.NewGuid().ToString();
            var chunks = SplitFile(e.FullPath, chunkSize).ToList();
            int totalChunks = chunks.Count;

            for (int i = 0; i < totalChunks; i++)
            {
                var messageObj = new
                {
                    FileId = fileId,
                    FileName = Path.GetFileName(e.FullPath),
                    ChunkNumber = i,
                    TotalChunks = totalChunks,
                    Data = Convert.ToBase64String(chunks[i])
                };
                var messageBody = JsonSerializer.Serialize(messageObj);
                var message = new ServiceBusMessage(messageBody)
                {
                    MessageId = $"{fileId}-{i}",
                    SessionId = fileId // Ensures all chunks for a file are grouped
                };
                await sender.SendMessageAsync(message);
            }
            Console.WriteLine($"Sent {totalChunks} chunks for {e.Name}");
        };
        watcher.EnableRaisingEvents = true;
        Console.WriteLine("Watching for new files...");
        Console.ReadLine();
        await sender.DisposeAsync();
        await client.DisposeAsync();
    }

    static IEnumerable<byte[]> SplitFile(string filePath, int chunkSize)
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
}