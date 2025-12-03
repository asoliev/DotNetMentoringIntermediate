using Azure.Messaging.ServiceBus;
using System.Collections.Concurrent;
using System.Text.Json;

internal class Program
{
    private static readonly ConcurrentDictionary<string, List<(int, byte[])>> fileChunks = new();

    private static async Task Main(string[] args)
    {
        string outputFolder = @"C:\Output";
        string queueName = "file_chunks";
        string serviceBusConnectionString = "Endpoint=sb://docprocessing-ns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=WUdorXDZVjCI4hVVvYHlWAl3V3vw03KrY+ASbPHWhTQ=";

        var client = new ServiceBusClient(serviceBusConnectionString);
        var processor = client.CreateSessionProcessor(queueName, new ServiceBusSessionProcessorOptions
        {
            MaxConcurrentSessions = 2,
            AutoCompleteMessages = false
        });

        processor.ProcessMessageAsync += async args =>
        {
            var body = args.Message.Body.ToString();
            var message = JsonSerializer.Deserialize<ChunkMessage>(body);
            var chunkData = Convert.FromBase64String(message.Data);

            var list = fileChunks.GetOrAdd(message.FileId, _ => new List<(int, byte[])>());
            list.Add((message.ChunkNumber, chunkData));

            if (list.Count == message.TotalChunks)
            {
                var ordered = list.OrderBy(x => x.Item1).Select(x => x.Item2).ToList();
                var outputPath = Path.Combine(outputFolder, message.FileName);
                using (var fs = new FileStream(outputPath, FileMode.Create))
                {
                    foreach (var chunk in ordered)
                        fs.Write(chunk, 0, chunk.Length);
                }
                fileChunks.TryRemove(message.FileId, out _);
                Console.WriteLine($"Reassembled and saved file: {outputPath}");
            }
            await args.CompleteMessageAsync(args.Message);
        };

        processor.ProcessErrorAsync += args =>
        {
            Console.WriteLine($"Error: {args.Exception}");
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync();
        Console.WriteLine("Listening for file chunks...");
        Console.ReadLine();
        await processor.StopProcessingAsync();
        await processor.DisposeAsync();
        await client.DisposeAsync();
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