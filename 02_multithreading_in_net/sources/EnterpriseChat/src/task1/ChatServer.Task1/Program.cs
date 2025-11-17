// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatServer.Task1;

internal static class Program
{
    private const int Port = 5050;
    private static readonly TcpListener Listener = new(IPAddress.Any, Port);
    private static readonly List<ClientHandler> Clients = [];
    private static readonly LinkedList<ChatMessage> History = new();
    private const int HistoryLimit = 50;
    private static readonly object SyncRoot = new();
    private static volatile bool _isRunning = true;

    private static void Main()
    {
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            Shutdown("Server is shutting down (Ctrl+C).");
        };

        Listener.Start();
        Console.WriteLine($"[SERVER] Listening on port {Port} ...");

        try
        {
            while (_isRunning)
            {
                var tcpClient = Listener.AcceptTcpClient();
                var handler = new ClientHandler(tcpClient, Broadcast, RemoveClient, SendHistory);
                lock (SyncRoot)
                {
                    Clients.Add(handler);
                }

                var thread = new Thread(handler.Run);
                handler.AttachThread(thread);
                thread.Start();
            }
        }
        catch (SocketException ex) when (!_isRunning)
        {
            Console.WriteLine($"[SERVER] Listener closed: {ex.Message}");
        }
    }

    private static void Broadcast(string line, ClientHandler? except = null)
    {
        lock (SyncRoot)
        {
            foreach (var client in Clients.ToArray())
                if (!ReferenceEquals(client, except))
                    client.TrySend(line);
        }
    }

    private static void SendHistory(ClientHandler client)
    {
        lock (SyncRoot)
        {
            foreach (var message in History)
            {
                client.TrySend($"HISTORY:{message.Timestamp:o}|{message.Sender}|{message.Text}");
            }
        }
    }

    internal static void RememberMessage(ChatMessage message)
    {
        lock (SyncRoot)
        {
            History.AddLast(message);
            if (History.Count > HistoryLimit)
                History.RemoveFirst();
        }
    }

    private static void RemoveClient(ClientHandler client, string reason)
    {
        lock (SyncRoot)
        {
            Clients.Remove(client);
        }

        if (!string.IsNullOrWhiteSpace(client.ClientName))
        {
            Console.WriteLine($"[SERVER] '{client.ClientName}' disconnected: {reason}");
            Broadcast($"SYSTEM:{client.ClientName} left the chat.");
        }

        client.Dispose();
    }

    private static void Shutdown(string reason)
    {
        if (!_isRunning) return;

        _isRunning = false;
        Console.WriteLine($"[SERVER] {reason}");
        Broadcast($"SERVER_SHUTDOWN:{DateTime.UtcNow:o}");
        Broadcast($"SYSTEM:{reason}");
        lock (SyncRoot)
        {
            foreach (var client in Clients.ToArray())
            {
                client.Dispose();
            }
            Clients.Clear();
        }

        Listener.Stop();
    }
}

internal sealed record ChatMessage(string Sender, string Text, DateTime Timestamp);

internal sealed class ClientHandler : IDisposable
{
    private readonly TcpClient _client;
    private readonly Action<string, ClientHandler?> _broadcast;
    private readonly Action<ClientHandler, string> _removeCallback;
    private readonly Action<ClientHandler> _sendHistory;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private Thread? _thread;

    public string ClientName { get; private set; } = string.Empty;

    public ClientHandler(
        TcpClient client,
        Action<string, ClientHandler?> broadcast,
        Action<ClientHandler, string> removeCallback,
        Action<ClientHandler> sendHistory)
    {
        _client = client;
        _broadcast = broadcast;
        _removeCallback = removeCallback;
        _sendHistory = sendHistory;
    }

    public void AttachThread(Thread thread) => _thread = thread;

    public void Run()
    {
        try
        {
            var stream = _client.GetStream();
            _reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            _writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true)
            {
                AutoFlush = true
            };

            // 1. Expect the name
            var first = _reader.ReadLine();
            if (first is null || !first.StartsWith("NAME:", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Client did not send NAME first.");
            }

            ClientName = first[5..].Trim();
            Console.WriteLine($"[SERVER] '{ClientName}' connected from {_client.Client.RemoteEndPoint}");
            _sendHistory(this);
            _broadcast($"SYSTEM:{ClientName} joined the chat.", this);

            // 2. Start reading chat messages
            string? line;
            while ((line = _reader.ReadLine()) is not null)
            {
                if (line.StartsWith("MSG:", StringComparison.Ordinal))
                {
                    var text = line[4..];
                    var msg = new ChatMessage(ClientName, text, DateTime.UtcNow);
                    Program.RememberMessage(msg);
                    _broadcast($"MSG:{msg.Timestamp:o}|{msg.Sender}|{msg.Text}", this);
                }
            }

            _removeCallback(this, "Client closed the connection.");
        }
        catch (Exception ex)
        {
            _removeCallback(this, ex.Message);
        }
    }

    public void TrySend(string line)
    {
        try
        {
            _writer?.WriteLine(line);
        }
        catch
        {
            _removeCallback(this, "Failed to send message.");
        }
    }

    public void Dispose()
    {
        _writer?.Dispose();
        _reader?.Dispose();
        _client.Close();
        if (_thread is { IsAlive: true })
        {
            _thread.Join(TimeSpan.FromMilliseconds(200));
        }
    }
}