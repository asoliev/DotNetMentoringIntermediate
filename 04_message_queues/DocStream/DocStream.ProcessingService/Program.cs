using Azure.Messaging.ServiceBus;
using DocStream.Shared;
using System.Collections.Concurrent;
using System.Text.Json;

namespace DocStream.ProcessingService.Consumer;

/// <summary> Consumer </summary>
public class Program
{
    #region State
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<int, byte[]>> fileChunks = new();
    private static readonly ConcurrentDictionary<string, (int line, int totalChunks, string fileName)> fileProgress = new();
    private static readonly ConcurrentDictionary<string, DateTime> fileStartTimes = new();
    private static readonly ConcurrentDictionary<string, long> fileBytesReceived = new();
    private static readonly ConcurrentDictionary<string, string> fileExpectedHashes = new();

    private static object consoleLock = new();
    private static int nextLine = 0;
    private static int barSize = 30;

    private class ChunkMessage
    {
        public required string FileId { get; set; }
        public required string FileName { get; set; }
        public int ChunkNumber { get; set; }
        public int TotalChunks { get; set; }
        public required string Data { get; set; }
        public string? FileHash { get; set; } // Optional, only in first chunk
    }
    #endregion

    /// <summary> Consumer </summary>
    public static async Task Main()
    {
        if (!Directory.Exists(AppConfig.OutputFolder))
            Directory.CreateDirectory(AppConfig.OutputFolder);

        var client = new ServiceBusClient(AppConfig.ServiceBusConnectionString);
        var sessionProcessorOptions = new ServiceBusSessionProcessorOptions
        {
            MaxConcurrentSessions = 20, // Try higher value
            PrefetchCount = 50,         // Try higher value
            AutoCompleteMessages = false
        };
        var processor = client.CreateSessionProcessor(AppConfig.QueueName, sessionProcessorOptions);

        processor.ProcessMessageAsync += OnProcessMessageAsync;
        processor.ProcessErrorAsync += OnProcessErrorAsync;

        await processor.StartProcessingAsync();
        Utils.PrintWithTimestamp("Listening for file chunks...");
        Console.ReadLine();

        await processor.StopProcessingAsync();
        await processor.DisposeAsync();
        await client.DisposeAsync();
    }

    private static async Task OnProcessMessageAsync(ProcessSessionMessageEventArgs args)
    {
        try
        {
            var message = DeserializeChunkMessage(args.Message.Body.ToString());
            var chunkData = Convert.FromBase64String(message.Data);

            StoreExpectedHash(message);

            var chunks = AddChunkToFile(message.FileId, message.ChunkNumber, chunkData);

            var (line, totalChunks, fileName) = GetOrCreateProgressInfo(message.FileId, message.TotalChunks, message.FileName);

            RecordStartTime(message.FileId);

            UpdateBytesReceived(message.FileId, chunkData.Length);

            int received = chunks.Count;
            int total = message.TotalChunks;
            var (eta, speedKBps) = ComputeTransferMetrics(message.FileId, received, total);

            PrintProgressBar(received, total, fileName, line + 3, eta, speedKBps);

            if (received == total)
            {
                var outputPath = ReassembleFile(message.FileId, fileName, chunks);

                HandleIntegrityCheck(message.FileId, fileName, outputPath, line + 5, speedKBps, received, total);

                CleanupFileState(message.FileId);
            }
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            PrintColoredLine($"Error processing chunk: {ex.Message}", ConsoleColor.Red);
            // Do not complete the message, so Service Bus will redeliver it
        }
    }

    private static Task OnProcessErrorAsync(ProcessErrorEventArgs args)
    {
        PrintColoredLine($"Error: {args.Exception}", ConsoleColor.Red);
        return Task.CompletedTask;
    }

    #region Helpers
    private static ChunkMessage DeserializeChunkMessage(string body)
    {
        var msg = JsonSerializer.Deserialize<ChunkMessage>(body);
        if (msg is null)
            throw new InvalidDataException("Failed to deserialize chunk message (null).");
        if (string.IsNullOrWhiteSpace(msg.FileId))
            throw new InvalidDataException("Chunk message missing FileId.");
        return msg;
    }

    private static void StoreExpectedHash(ChunkMessage message)
    {
        if (!string.IsNullOrEmpty(message.FileHash))
            fileExpectedHashes[message.FileId] = message.FileHash;
    }

    private static ConcurrentDictionary<int, byte[]> AddChunkToFile(string fileId, int chunkNumber, byte[] chunkData)
    {
        var dict = fileChunks.GetOrAdd(fileId, _ => new ConcurrentDictionary<int, byte[]>());
        // Use indexer to add or replace safely
        dict[chunkNumber] = chunkData;
        return dict;
    }

    private static (int line, int totalChunks, string fileName) GetOrCreateProgressInfo(string fileId, int totalChunks, string fileName)
    {
        return fileProgress.GetOrAdd(fileId, _ =>
        {
            int line;
            lock (consoleLock)
            {
                line = nextLine++;
                Console.WriteLine(); // Reserve a line
            }
            PrintColoredLine($"Detected new file: {fileName} (FileId: {fileId})", ConsoleColor.White, line + 1);
            return (line, totalChunks, fileName);
        });
    }

    private static void RecordStartTime(string fileId)
        => fileStartTimes.TryAdd(fileId, DateTime.UtcNow);

    private static void UpdateBytesReceived(string fileId, int chunkLength)
        => fileBytesReceived.AddOrUpdate(fileId, chunkLength, (k, v) => v + chunkLength);

    private static (TimeSpan? eta, double speedKBps) ComputeTransferMetrics(string fileId, int received, int total)
    {
        if (received <= 1)
            return (null, 0);

        var start = fileStartTimes[fileId];
        var elapsed = DateTime.UtcNow - start;
        double avgPerChunk = elapsed.TotalSeconds / received;
        int remaining = total - received;
        TimeSpan eta = TimeSpan.FromSeconds(avgPerChunk * remaining);

        long bytes = fileBytesReceived[fileId];
        double speedKBps = elapsed.TotalSeconds > 0 ? (bytes / 1024.0) / elapsed.TotalSeconds : 0;

        return (eta, speedKBps);
    }

    private static void PrintProgressBar(int received, int total, string fileName, int consoleLine, TimeSpan? eta, double speedKBps, bool completed = false)
    {
        double percent = (double)received / total;
        int filled = (int)(percent * barSize);
        int empty = barSize - filled;
        string bar = new string('█', filled) + new string('-', empty);

        string etaStr = eta.HasValue ? $" | ETA: {eta.Value:mm\\:ss}" : "";
        string speedStr = speedKBps > 0 ? $" | {speedKBps:F2} Kb/s" : "";

        string progress = completed
            ? $"Completed '{fileName}' and saved to output."
            : $"Receiving '{fileName}': [{bar}] {percent * 100:F1}% ({received}/{total} chunks){speedStr}{etaStr}";

        ConsoleColor color = completed ? ConsoleColor.Green
            : percent >= 0.8 ? ConsoleColor.Yellow
            : ConsoleColor.Cyan;

        PrintColoredLine(progress, color, consoleLine);
    }

    private static void PrintColoredLine(string message, ConsoleColor color, int? line = null)
    {
        lock (consoleLock)
        {
            if (line.HasValue)
                Console.SetCursorPosition(0, line.Value);
            Console.ForegroundColor = color;
            Utils.PrintWithTimestamp(message.PadRight(Console.WindowWidth - 1));
            Console.ResetColor();
        }
    }
    private static string ReassembleFile(string fileId, string fileName, ConcurrentDictionary<int, byte[]> chunks)
    {
        var ordered = chunks.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
        var outputPath = Path.Combine(AppConfig.OutputFolder, fileName);
        using (var fs = new FileStream(outputPath, FileMode.Create))
        {
            foreach (var chunk in ordered)
                fs.Write(chunk, 0, chunk.Length);
        }
        return outputPath;
    }
    private static void HandleIntegrityCheck(string fileId, string fileName, string outputPath, int line, double speedKBps, int received, int total)
    {
        string computedHash = Utils.ComputeSHA256(outputPath);
        bool valid = fileExpectedHashes.TryGetValue(fileId, out var expectedHash) && expectedHash == computedHash;

        PrintProgressBar(received, total, fileName, line, null, speedKBps, completed: true);

        if (valid)
            PrintColoredLine($"Integrity check PASSED for '{fileName}' (SHA256: {computedHash})", ConsoleColor.Green, line);
        else
            PrintColoredLine($"Integrity check FAILED for '{fileName}' (expected: {expectedHash}, got: {computedHash})", ConsoleColor.Red, line);
    }
    private static void CleanupFileState(string fileId)
    {
        fileProgress.TryRemove(fileId, out _);
        fileStartTimes.TryRemove(fileId, out _);
        fileBytesReceived.TryRemove(fileId, out _);
        fileExpectedHashes.TryRemove(fileId, out _);
        fileChunks.TryRemove(fileId, out _);
    }
    #endregion
}