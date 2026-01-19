// See https://aka.ms/new-console-template for more information
using System.Net.Sockets;
using System.Text;

namespace ChatClient.Task1;

internal static class Program
{
    private const string Host = "127.0.0.1";
    private const int Port = 5050;
    private static readonly string[] Messages =
    {
        "Hey team!",
        "How is the sprint going?",
        "Reminder: stand-up at 10:00.",
        "Pushing a hotfix now.",
        "Lunch time!",
        "CI pipeline is green.",
        "Anyone free for code review?"
    };

    private static volatile bool _isRunning = true;

    private static void Main()
    {
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            _isRunning = false;
            Console.WriteLine("[CLIENT] Stopping...");
        };

        using var log = File.AppendText("chat-log.txt");
        log.AutoFlush = true;

        while (_isRunning)
        {
            var clientName = $"Bot_{Guid.NewGuid():N}".Substring(0, 12);
            try
            {
                using var tcpClient = new TcpClient();
                tcpClient.Connect(Host, Port);
                var stream = tcpClient.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
                using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true)
                {
                    AutoFlush = true
                };

                writer.WriteLine($"NAME:{clientName}");
                Console.WriteLine($"[CLIENT] Connected as '{clientName}'.");

                var receiverThread = new Thread(() => ReceiveLoop(reader, log));
                receiverThread.Start();

                var messagesToSend = Random.Shared.Next(2, 6);
                for (var i = 0; i < messagesToSend && _isRunning; i++)
                {
                    var message = Messages[Random.Shared.Next(Messages.Length)];
                    writer.WriteLine($"MSG:{message}");
                    Console.WriteLine($"[CLIENT] Sent: {message}");
                    Thread.Sleep(Random.Shared.Next(400, 1200));
                }

                // give some time to collect responses
                Thread.Sleep(Random.Shared.Next(500, 1500));

                tcpClient.Close(); // triggers EOF on server side
                receiverThread.Join();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT] Error: {ex.Message}");
            }

            Thread.Sleep(Random.Shared.Next(1500, 2500));
        }

        Console.WriteLine("[CLIENT] Bye.");
    }

    private static void ReceiveLoop(StreamReader reader, StreamWriter log)
    {
        try
        {
            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                Console.WriteLine($"[CLIENT] << {line}");
                log.WriteLine(line);
            }
        }
        catch (IOException)
        {
            // network stream closed
        }
    }
}