using Azure.Messaging.ServiceBus;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;

namespace DocStream.ProcessingService.Consumer;

/// <summary> Consumer </summary>
class Program
{
    static readonly ConcurrentDictionary<string, List<(int, byte[])>> fileChunks = new();
    static ConcurrentDictionary<string, (int line, int totalChunks, string fileName)> fileProgress = new();
    static ConcurrentDictionary<string, DateTime> fileStartTimes = new();
    static ConcurrentDictionary<string, long> fileBytesReceived = new();
    static ConcurrentDictionary<string, string> fileExpectedHashes = new();

    static object consoleLock = new();
    static int nextLine = 0;
    static string outputFolder;
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

    /// <summary> Consumer </summary>
    public static async Task Main()
    {
        outputFolder = @"C:\Users\Abdukodir_Soliev\Downloads\MessageQueue\Output";
        string queueName = "filechunks";
        string serviceBusConnectionString = "Endpoint=sb://docprocessing-ns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=WUdorXDZVjCI4hVVvYHlWAl3V3vw03KrY+ASbPHWhTQ=";

        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);

        var client = new ServiceBusClient(serviceBusConnectionString);
        var processor = client.CreateSessionProcessor(queueName, new ServiceBusSessionProcessorOptions
        {
            MaxConcurrentSessions = 2,
            AutoCompleteMessages = false
        });

        processor.ProcessMessageAsync += OnProcessMessageAsync;
        processor.ProcessErrorAsync += OnProcessErrorAsync;

        await processor.StartProcessingAsync();
        PrintWithTimestamp("Listening for file chunks...");
        Console.ReadLine();

        await processor.StopProcessingAsync();
        await processor.DisposeAsync();
        await client.DisposeAsync();
    }

    private static async Task OnProcessMessageAsync(ProcessSessionMessageEventArgs args)
    {
        try
        {
            //Console.WriteLine($"[{DateTime.Now:yyyy MMM dd HH:mm:ss}] ProcessMessage");
            var body = args.Message.Body.ToString();
            var message = JsonSerializer.Deserialize<ChunkMessage>(body);
            var chunkData = Convert.FromBase64String(message.Data);

            // Store expected hash if present (first chunk)
            if (!string.IsNullOrEmpty(message.FileHash))
                fileExpectedHashes[message.FileId] = message.FileHash;

            List<(int, byte[])> list = fileChunks.GetOrAdd(message.FileId, _ => []);
            list.Add((message.ChunkNumber, chunkData));

            // Assign a line for this file if not already done
            var progressInfo = fileProgress.GetOrAdd(message.FileId, _ =>
            {
                int line;
                lock (consoleLock)
                {
                    line = nextLine++;
                    Console.WriteLine(); // Reserve a line
                }
                PrintWithTimestamp($"Detected new file: {message.FileName} (FileId: {message.FileId})");
                return (line, message.TotalChunks, message.FileName);
            });

            // Record start time for ETA calculation
            fileStartTimes.TryAdd(message.FileId, DateTime.UtcNow);

            // Track bytes received
            fileBytesReceived.AddOrUpdate(message.FileId, chunkData.Length, (k, v) => v + chunkData.Length);
            
            int received = list.Count;
            int total = message.TotalChunks;
            var (eta, speedKBps) = ComputeTransferMetrics(message.FileId, received, total);

            PrintProgressBar(received, total, progressInfo.fileName, progressInfo.line, eta, speedKBps);

            if (received == total)
            {
                var ordered = list.OrderBy(x => x.Item1).Select(x => x.Item2).ToList();
                var outputPath = Path.Combine(outputFolder, message.FileName);
                using (var fs = new FileStream(outputPath, FileMode.Create))
                {
                    foreach (var chunk in ordered)
                        fs.Write(chunk, 0, chunk.Length);
                }
                fileChunks.TryRemove(message.FileId, out _);

                // Integrity check
                string computedHash = ComputeSHA256(outputPath);
                bool valid = fileExpectedHashes.TryGetValue(message.FileId, out var expectedHash) && expectedHash == computedHash;

                // Print completion message on the same line
                PrintProgressBar(received, total, progressInfo.fileName, progressInfo.line, null, speedKBps, completed: true);

                lock (consoleLock)
                {
                    Console.SetCursorPosition(0, progressInfo.line + 1);
                    if (valid)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Integrity check PASSED for '{progressInfo.fileName}' (SHA256: {computedHash})");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Integrity check FAILED for '{progressInfo.fileName}' (expected: {expectedHash}, got: {computedHash})");
                    }
                    Console.ResetColor();
                }

                CleanupFileState(message.FileId);
            }
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error processing chunk: {ex.Message}");
                Console.ResetColor();
            }
        }
    }

    private static Task OnProcessErrorAsync(ProcessErrorEventArgs args)
    {
        lock (consoleLock)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {args.Exception}");
            Console.ResetColor();
        }
        return Task.CompletedTask;
    }

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

    static void PrintWithTimestamp(string message)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy/MMM/dd-HH:mm:ss}] {message}");
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

        lock (consoleLock)
        {
            int currentLine = Console.CursorTop;
            Console.SetCursorPosition(0, line);

            // Color: Green for completed, Yellow for >80%, Cyan otherwise
            if (completed)
                Console.ForegroundColor = ConsoleColor.Green;
            else if (percent >= 0.8)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else
                Console.ForegroundColor = ConsoleColor.Cyan;

            PrintWithTimestamp(progress.PadRight(Console.WindowWidth - 1));
            Console.SetCursorPosition(0, currentLine);
        }
    }

    private static void PrintColoredLine(string message, ConsoleColor color, int? line = null)
    {
        lock (consoleLock)
        {
            if (line.HasValue)
                Console.SetCursorPosition(0, line.Value);
            Console.ForegroundColor = color;
            Console.WriteLine(message.PadRight(Console.WindowWidth - 1));
            Console.ResetColor();
        }
    }
    private static string ReassembleFile(string fileName, List<(int, byte[])> chunks)
    {
        var ordered = chunks.OrderBy(x => x.Item1).Select(x => x.Item2).ToList();
        var outputPath = Path.Combine(outputFolder, fileName);
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
        string computedHash = ComputeSHA256(outputPath);
        bool valid = fileExpectedHashes.TryGetValue(fileId, out var expectedHash) && expectedHash == computedHash;

        PrintProgressBar(received, total, fileName, line, null, speedMBps, completed: true);

        if (valid)
            PrintColoredLine($"Integrity check PASSED for '{fileName}' (SHA256: {computedHash})", ConsoleColor.Green, line + 1);
        else
            PrintColoredLine($"Integrity check FAILED for '{fileName}' (expected: {expectedHash}, got: {computedHash})", ConsoleColor.Red, line + 1);
    }
    private static void CleanupFileState(string fileId)
    {
        fileProgress.TryRemove(fileId, out _);
        fileStartTimes.TryRemove(fileId, out _);
        fileBytesReceived.TryRemove(fileId, out _);
        fileExpectedHashes.TryRemove(fileId, out _);
    }

    private static string ComputeSHA256(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return Convert.ToHexStringLower(hash);
    }
}