// See https://aka.ms/new-console-template for more information
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatServer.Task2;

internal static class Program
{
    private const int Port = 5050;
    private static readonly TcpListener Listener =
        new(IPAddress.Any, Port);
    private static readonly ConcurrentDictionary<Guid, ClientContext> Clients = new();
    private static readonly LinkedList<ChatMessage> History = new();
    private const int HistoryLimit = 50;
    private static readonly SemaphoreSlim HistoryLock = new(1, 1);
    private static readonly CancellationTokenSource ShutdownCts = new();

    private static async Task Main()
    {
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            ShutdownCts.Cancel();
        };

        Listener.Start();
        Console.WriteLine($"[SERVER-ASYNC] Listening on port {Port} ...");

        var acceptTask = AcceptLoopAsync(ShutdownCts.Token);

        await acceptTask.ConfigureAwait(false);
        Console.WriteLine("[SERVER-ASYNC] Stopped accepting new clients.");

        ShutdownCts.Dispose(); // Dispose ShutdownCts when no longer needed
    }

    private static async Task AcceptLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var tcpClient = await Listener.AcceptTcpClientAsync(token).ConfigureAwait(false);
                _ = Task.Run(() => HandleClientAsync(tcpClient, token), token);
            }
        }
        catch (OperationCanceledException)
        {
            // normal shutdown
        }
        finally
        {
            Listener.Stop();
            try
            {
                await BroadcastAsync($"SERVER_SHUTDOWN:{DateTime.UtcNow:o}", default).ConfigureAwait(false);
            }
            catch
            {
                // Ignore exceptions during shutdown broadcast to ensure cleanup always runs
            }

            foreach (var client in Clients.Values)
            {
                client.Dispose();
            }
        }
    }

    private static async Task HandleClientAsync(TcpClient tcpClient, CancellationToken token)
    {
        using var stream = tcpClient.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };

        string? nameLine;
        try
        {
            nameLine = await reader.ReadLineAsync().ConfigureAwait(false);
        }
        catch (IOException)
        {
            return;
        }

        if (nameLine is null || !nameLine.StartsWith("NAME:", StringComparison.Ordinal))
        {
            return;
        }

        var clientName = nameLine[5..].Trim();
        var context = new ClientContext(Guid.NewGuid(), clientName, tcpClient, writer);

        if (!Clients.TryAdd(context.Id, context))
        {
            return;
        }

        Console.WriteLine($"[SERVER-ASYNC] '{clientName}' connected.");

        try
        {
            await SendHistoryAsync(context).ConfigureAwait(false);
            await BroadcastAsync($"SYSTEM:{clientName} joined the chat.", context).ConfigureAwait(false);

            while (!token.IsCancellationRequested && tcpClient.Connected)
            {
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line is null)
                {
                    break;
                }

                if (!line.StartsWith("MSG:", StringComparison.Ordinal))
                {
                    continue;
                }

                var text = line[4..];
                var msg = new ChatMessage(clientName, text, DateTime.UtcNow);
                await RememberAsync(msg).ConfigureAwait(false);
                await BroadcastAsync($"MSG:{msg.Timestamp:o}|{msg.Sender}|{msg.Text}", default, context.Id)
                    .ConfigureAwait(false);
            }
        }
        catch (IOException)
        {
            // client dropped
        }
        finally
        {
            Clients.TryRemove(context.Id, out _);
            context.Dispose();
            await BroadcastAsync($"SYSTEM:{clientName} left the chat.").ConfigureAwait(false);
            Console.WriteLine($"[SERVER-ASYNC] '{clientName}' disconnected.");
        }
    }

    private static async Task BroadcastAsync(
        string line,
        ClientContext? exclude = default,
        Guid? excludeId = default)
    {
        List<Task> tasks = new();
        foreach (var (id, client) in Clients)
        {
            if (exclude is not null && ReferenceEquals(client, exclude)) continue;
            if (excludeId.HasValue && id == excludeId.Value) continue;
            tasks.Add(client.SendAsync(line));
        }
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private static async Task SendHistoryAsync(ClientContext client)
    {
        await HistoryLock.WaitAsync().ConfigureAwait(false);
        try
        {
            foreach (var item in History)
            {
                await client.SendAsync($"HISTORY:{item.Timestamp:o}|{item.Sender}|{item.Text}")
                    .ConfigureAwait(false);
            }
        }
        finally
        {
            HistoryLock.Release();
        }
    }

    private static async Task RememberAsync(ChatMessage message)
    {
        await HistoryLock.WaitAsync().ConfigureAwait(false);
        try
        {
            History.AddLast(message);
            if (History.Count > HistoryLimit)
            {
                History.RemoveFirst();
            }
        }
        finally
        {
            HistoryLock.Release();
        }
    }
}

internal sealed class ClientContext : IDisposable
{
    public Guid Id { get; }
    public string Name { get; }
    private readonly TcpClient _client;
    private readonly StreamWriter _writer;

    public ClientContext(Guid id, string name, TcpClient client, StreamWriter writer)
    {
        Id = id;
        Name = name;
        _client = client;
        _writer = writer;
    }

    public Task SendAsync(string line)
    {
        return _writer.WriteLineAsync(line);
    }

    public void Dispose()
    {
        _writer.Dispose();
        _client.Close();
    }
}

internal sealed record ChatMessage(string Sender, string Text, DateTime Timestamp);