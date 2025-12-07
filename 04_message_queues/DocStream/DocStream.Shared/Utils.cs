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
}