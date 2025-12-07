using System.Security.Cryptography;

namespace DocStream.Shared;

public static class Utils
{
    public static string ComputeSHA256(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return Convert.ToHexStringLower(hash);
    }

    /// <summary>
    /// Asynchronously waits until the specified file is no longer locked and is ready for access.
    /// </summary>
    /// <remarks>This method periodically checks the file's lock status and returns only when the file is
    /// available. Use this method to ensure that a file is not in use by another process before attempting to read or
    /// write to it.</remarks>
    /// <param name="filePath">The full path of the file to check for readiness. Cannot be null or empty.</param>
    /// <returns>A task that completes when the file at the specified path is ready for access.</returns>
    public static async Task WaitForFileReady(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        while (IsFileLocked(filePath))
            await Task.Delay(500);
    }
    public static bool IsFileLocked(string file)
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

    public static IEnumerable<byte[]> SplitFile(string filePath, int chunkSize)
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

    public static void PrintColoredLineWithLock(string message, ConsoleColor color, int? line = null, object? consoleLock = null)
    {
        if (consoleLock != null)
            lock (consoleLock)
                PrintColoredLine(message, color, line);
        else
            PrintColoredLine(message, color, line);
    }

    private static void PrintColoredLine(string message, ConsoleColor color, int? line = null)
    {
        if (line.HasValue)
            Console.SetCursorPosition(0, line.Value);
        Console.ForegroundColor = color;
        PrintWithTimestamp(message.PadRight(Console.WindowWidth - 1));
        Console.ResetColor();
    }

    public static void PrintWithTimestamp(string message) => Console.WriteLine($"[{DateTime.Now:yyyy/MMM/dd-HH:mm:ss}] {message}");

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
        int filled = (int)(percent * AppConfig.BarSize);
        int empty = AppConfig.BarSize - filled;
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
}