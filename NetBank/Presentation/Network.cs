namespace NetBank.Presentation;

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class TcpOrchestratorServer
{
    private readonly IOrchestrator _orchestrator;
    private readonly int _port;
    private readonly TimeSpan _inactivityTimeout;

    public TcpOrchestratorServer(IOrchestrator orchestrator, int port, TimeSpan inactivityTimeout)
    {
        _orchestrator = orchestrator;
        _port = port;
        _inactivityTimeout = inactivityTimeout;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        var listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();

        try
        {
            while (!ct.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(ct);
                _ = HandleClientAsync(client, ct);
            }
        }
        finally
        {
            listener.Stop();
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken globalCt)
    {
        using (client)
        await using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8))
        await using (var writer = new StreamWriter(stream, Encoding.UTF8))
        {
            writer.AutoFlush = true;
            try
            {
                while (client.Connected)
                {
                    using (var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(globalCt))
                    {
                        timeoutCts.CancelAfter(_inactivityTimeout);
                        string? command = await reader.ReadLineAsync(timeoutCts.Token);
                        if (string.IsNullOrEmpty(command)) break;
                        string result = await _orchestrator.ExecuteTextCommand(command);
                        await writer.WriteLineAsync(result);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Client disconnected due to inactivity timeout.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}