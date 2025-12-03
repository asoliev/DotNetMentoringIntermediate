using RabbitMQ.Client;
using System.Text.Json;

internal class Program
{
    private static async Task Main(string[] args)
    {
        string inputFolder = @"C:\Input";
        string queueName = "file_chunks";
        int chunkSize = 1024 * 1024 * 4; // 4MB per chunk

        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

        var watcher = new FileSystemWatcher(inputFolder, "*.pdf");
        watcher.Created += async (s, e) =>
        {
            Console.WriteLine($"Detected new file: {e.FullPath}");
            var fileId = Guid.NewGuid().ToString();
            var chunks = SplitFile(e.FullPath, chunkSize).ToList();
            int totalChunks = chunks.Count;

            for (int i = 0; i < totalChunks; i++)
            {
                var message = new
                {
                    FileId = fileId,
                    FileName = Path.GetFileName(e.FullPath),
                    ChunkNumber = i,
                    TotalChunks = totalChunks,
                    Data = Convert.ToBase64String(chunks[i])
                };
                var body = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                await channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: body);
            }
            Console.WriteLine($"Sent {totalChunks} chunks for {e.Name}");
        };
        watcher.EnableRaisingEvents = true;
        Console.WriteLine("Watching for new files...");
        Console.ReadLine();
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