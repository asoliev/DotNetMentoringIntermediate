using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text.Json;

internal class Program
{
    private static readonly ConcurrentDictionary<string, List<(int, byte[])>> fileChunks = new();

    private static async Task Main(string[] args)
    {
        string outputFolder = @"C:\Output";
        string queueName = "file_chunks";

        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (model, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            if (body == null || body.Length == 0)
                return Task.CompletedTask;

            var body_str = System.Text.Encoding.UTF8.GetString(body);
            if (string.IsNullOrEmpty(body_str))
                return Task.CompletedTask;

            var message = JsonSerializer.Deserialize<ChunkMessage>(body_str);
            if (message == null)
                return Task.CompletedTask;

            var chunkData = Convert.FromBase64String(message.Data);

            List<(int, byte[])> list = fileChunks.GetOrAdd(message.FileId, _ => []);
            list.Add((message.ChunkNumber, chunkData));

            if (list.Count == message.TotalChunks)
            {
                var ordered = list.OrderBy(x => x.Item1).Select(x => x.Item2).ToList();
                var outputPath = Path.Combine(outputFolder, message
.FileName);
                using (var fs = new FileStream(outputPath, FileMode.Create))
                {
                    foreach (var chunk in ordered)
                        fs.Write(chunk, 0, chunk.Length);
                }
                fileChunks.TryRemove(message.FileId, out _);
                Console.WriteLine($"Reassembled and saved file: {outputPath}");
            }
            return Task.CompletedTask;
        };
        await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
        Console.WriteLine("Listening for file chunks...");
        Console.ReadLine();
    }

    public class ChunkMessage
    {
        public required string FileId { get; set; }
        public required string FileName { get; set; }
        public int ChunkNumber { get; set; }
        public int TotalChunks { get; set; }
        public required string Data { get; set; }
    }
}