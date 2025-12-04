using Azure.Messaging.ServiceBus;
using DocStream.Shared;
using Microsoft.Azure.Amqp.Framing;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;

namespace DocStream.ProcessingService.Consumer;

/// <summary> Consumer </summary>
class Program
{
    #region State
    static readonly ConcurrentDictionary<string, List<(int, byte[])>> fileChunks = new();
    static ConcurrentDictionary<string, (int line, int totalChunks, string fileName)> fileProgress = new();
    static ConcurrentDictionary<string, DateTime> fileStartTimes = new();
    static ConcurrentDictionary<string, long> fileBytesReceived = new();
    static ConcurrentDictionary<string, string> fileExpectedHashes = new();

    static object consoleLock = new();
    static int nextLine = 0;
    static int barSize = 30;

    public class ChunkMessage
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

            var list = AddChunkToFile(message.FileId, message.ChunkNumber, chunkData);

            var progressInfo = GetOrCreateProgressInfo(message.FileId, message.TotalChunks, message.FileName);

            RecordStartTime(message.FileId);

            UpdateBytesReceived(message.FileId, chunkData.Length);

            int received = list.Count;
            int total = message.TotalChunks;
            var (eta, speedKBps) = ComputeTransferMetrics(message.FileId, received, total);

            PrintProgressBar(received, total, progressInfo.fileName, progressInfo.line + 3, eta, speedKBps);

            if (received == total)
            {
                var outputPath = ReassembleFile(progressInfo.fileName, list);

                HandleIntegrityCheck(message.FileId, progressInfo.fileName, outputPath, progressInfo.line + 5, speedKBps, received, total);

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
    private static ChunkMessage DeserializeChunkMessage(string body) => JsonSerializer.Deserialize<ChunkMessage>(body);

    private static void StoreExpectedHash(ChunkMessage message)
    {
        if (!string.IsNullOrEmpty(message.FileHash))
            fileExpectedHashes[message.FileId] = message.FileHash;
    }

    private static List<(int, byte[])> AddChunkToFile(string fileId, int chunkNumber, byte[] chunkData)
    {
        List<(int, byte[])> list = fileChunks.GetOrAdd(fileId, _ => []);
        list.Add((chunkNumber, chunkData));
        return list;
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

    private static (TimeSpan? eta, double speedMBps) ComputeTransferMetrics(string fileId, int received, int total)
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

    /// <summary>
    /// Displays a progress bar in the console to indicate the status of a file reception operation.
    /// </summary>
    /// <remarks>This method updates the progress bar in-place at the specified console line, allowing for
    /// real-time feedback during file reception. The method is thread-safe and intended for use in console applications
    /// where visual progress indication is required.</remarks>
    /// <param name="received">The number of data chunks that have been received so far. Must be between 0 and <paramref name="total"/>.</param>
    /// <param name="total">The total number of data chunks expected to be received. Must be greater than zero.</param>
    /// <param name="fileName">The name of the file being received. Used for display purposes in the progress bar.</param>
    /// <param name="line">The console line number at which the progress bar should be rendered.</param>
    /// <param name="eta">An optional estimated time remaining for the operation. If specified, it can be used to inform the user of the
    /// expected completion time.</param>
    /// <param name="speedMBps">The current speed of data reception in kilobytes per second. Used for display purposes.</param>
    /// <param name="completed">Indicates whether the file reception has been completed. If true, the progress bar will indicate completion.</param>
    /// <param name="barSize">The total width, in characters, of the progress bar. Defaults to 30 if not specified.</param>
    private static void PrintProgressBar(int received, int total, string fileName, int line, TimeSpan? eta, double speedMBps, bool completed = false)
    {
        double percent = (double)received / total;
        int filled = (int)(percent * barSize);
        int empty = barSize - filled;
        string bar = new string('█', filled) + new string('-', empty);

        string etaStr = eta.HasValue ? $" | ETA: {eta.Value:mm\\:ss}" : "";
        string speedStr = speedMBps > 0 ? $" | {speedMBps:F2} Kb/s" : "";

        string progress = completed
            ? $"Completed '{fileName}' and saved to output."
            : $"Receiving '{fileName}': [{bar}] {percent * 100:F1}% ({received}/{total} chunks){speedStr}{etaStr}";

        ConsoleColor color = completed ? ConsoleColor.Green
            : percent >= 0.8 ? ConsoleColor.Yellow
            : ConsoleColor.Cyan;

        PrintColoredLine(progress, color, line);
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
    private static string ReassembleFile(string fileName, List<(int, byte[])> chunks)
    {
        var ordered = chunks.OrderBy(x => x.Item1).Select(x => x.Item2).ToList();
        var outputPath = Path.Combine(AppConfig.OutputFolder, fileName);
        using (var fs = new FileStream(outputPath, FileMode.Create))
        {
            foreach (var chunk in ordered)
                fs.Write(chunk, 0, chunk.Length);
        }
        fileChunks.TryRemove(fileName, out _);
        return outputPath;
    }
    private static void HandleIntegrityCheck(string fileId, string fileName, string outputPath, int line, double speedMBps, int received, int total)
    {
        string computedHash = Utils.ComputeSHA256(outputPath);
        bool valid = fileExpectedHashes.TryGetValue(fileId, out var expectedHash) && expectedHash == computedHash;

        PrintProgressBar(received, total, fileName, line, null, speedMBps, completed: true);

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
    }
    #endregion
}