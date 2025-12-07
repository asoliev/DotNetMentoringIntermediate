// See https://aka.ms/new-console-template for more information
using System.Net.Sockets;
using System.Text;

namespace ChatClient.Task2;

internal static class Program
{
    private const string Host = "127.0.0.1";
    private const int Port = 5050;
    private static readonly string[] Messages =
    {
        "Async bot reporting in.",
        "TPL makes this easy!",
        "Awaiting further instructions...",
        "Ping from Task-based bot.",
        "Love these async streams."
    };
    private static readonly Random Random = new();
    private static readonly CancellationTokenSource Cts = new();

    private static async Task Main()
    {
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            Cts.Cancel();
            Console.WriteLine("[CLIENT-ASYNC] Stop requested.");
        };

        using var log = File.AppendText("chat-log-async.txt");
        log.AutoFlush = true;

        try
        {
            while (!Cts.IsCancellationRequested)
            {
                var name = $"AsyncBot_{Guid.NewGuid():N}".Substring(0, 14);
                try
                {
                    using var client = new TcpClient();
                    await client.ConnectAsync(Host, Port, Cts.Token);
                    using var stream = client.GetStream();
                    using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
                    using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true)
                    {
                        AutoFlush = true
                    };

                    await writer.WriteLineAsync($"NAME:{name}");
                    Console.WriteLine($"[CLIENT-ASYNC] Connected as {name}");

                    Task receiveTask = ReceiveLoopAsync(reader, log, Cts.Token);

                    var toSend = Random.Next(3, 7);
                    for (var i = 0; i < toSend && !Cts.IsCancellationRequested; i++)
                    {
                        var message = Messages[Random.Next(Messages.Length)];
                        await writer.WriteLineAsync($"MSG:{message}");
                        Console.WriteLine($"[CLIENT-ASYNC] Sent: {message}");
                        await Task.Delay(Random.Next(400, 1200), Cts.Token);
                    }

                    await Task.Delay(Random.Next(600, 1500), Cts.Token);
                    client.Close();
                    await receiveTask;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CLIENT-ASYNC] Error: {ex.Message}");
                }

                await Task.Delay(Random.Next(1200, 2000), Cts.Token);
            }
        }
        finally
        {
            Cts.Dispose();
        }

        Console.WriteLine("[CLIENT-ASYNC] Bye.");
    }

    private static async Task ReceiveLoopAsync(StreamReader reader, StreamWriter log, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(token).ConfigureAwait(false);
                if (line is null) break;
                Console.WriteLine($"[CLIENT-ASYNC] << {line}");
                await log.WriteLineAsync(line).ConfigureAwait(false);

                if (line.StartsWith("SERVER_SHUTDOWN", StringComparison.Ordinal))
                {
                    break;
                }
            }
        }
        catch (IOException)
        {
            // connection dropped
        }
    }
}