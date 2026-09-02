using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UefiTools.UefiEditor;

public class UefiEditorServer
{
    private readonly ILogger<UefiEditorServer> _logger;

    public UefiEditorServer(ILogger<UefiEditorServer> logger)
    {
        _logger = logger;
    }

    public int Serve(string html, string listenAddress)
    {
        var endpoint = ParseEndpoint(listenAddress);
        using var cancellationSource = new CancellationTokenSource();
        using var listener = new TcpListener(endpoint);
        ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationSource.Cancel();
        };

        Console.CancelKeyPress += cancelHandler;
        try
        {
            listener.Start();
            _logger.LogInformation("Serving UEFI editor at http://{address}. Press Ctrl+C to stop.", listenAddress);

            while (!cancellationSource.IsCancellationRequested)
            {
                try
                {
                    using var client = listener.AcceptTcpClientAsync(cancellationSource.Token).GetAwaiter().GetResult();
                    ServeClientAsync(client, html, cancellationSource.Token).GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    // Ctrl+C stops the accept loop.
                }
            }
        }
        finally
        {
            Console.CancelKeyPress -= cancelHandler;
        }

        return 0;
    }

    private static async Task ServeClientAsync(TcpClient client, string html, CancellationToken cancellationToken)
    {
        await using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
        var requestLine = await reader.ReadLineAsync(cancellationToken);
        if (requestLine == null)
        {
            return;
        }

        while (!string.IsNullOrEmpty(await reader.ReadLineAsync(cancellationToken)))
        {
        }

        if (requestLine.StartsWith("GET / ", StringComparison.Ordinal))
        {
            await WriteResponseAsync(stream, "200 OK", "text/html; charset=utf-8", html, cancellationToken);
        }
        else if (requestLine.StartsWith("GET /favicon.ico ", StringComparison.Ordinal))
        {
            await WriteResponseAsync(stream, "204 No Content", null, null, cancellationToken);
        }
        else
        {
            await WriteResponseAsync(stream, "404 Not Found", "text/plain; charset=utf-8", "Not found", cancellationToken);
        }
    }

    private static async Task WriteResponseAsync(NetworkStream stream, string status, string? contentType,
        string? body, CancellationToken cancellationToken)
    {
        var content = body == null ? [] : Encoding.UTF8.GetBytes(body);
        var headers = $"HTTP/1.1 {status}\r\n" +
                      $"Content-Length: {content.Length}\r\n" +
                      "Connection: close\r\n" +
                      (contentType == null ? string.Empty : $"Content-Type: {contentType}\r\n") +
                      "\r\n";
        await stream.WriteAsync(Encoding.ASCII.GetBytes(headers), cancellationToken);
        await stream.WriteAsync(content, cancellationToken);
    }

    public static IPEndPoint ParseEndpoint(string listenAddress)
    {
        var separator = listenAddress.LastIndexOf(':');
        if (separator <= 0 || separator == listenAddress.Length - 1 ||
            !int.TryParse(listenAddress[(separator + 1)..], out var port) || port is < 1 or > 65535)
        {
            throw new ArgumentException("Serve address must use host:port, for example 127.0.0.1:4060.", nameof(listenAddress));
        }

        var host = listenAddress[..separator];
        var address = host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            ? IPAddress.Loopback
            : IPAddress.TryParse(host, out var parsed) ? parsed : throw new ArgumentException(
                "Serve host must be localhost or a loopback IP address.", nameof(listenAddress));
        if (!IPAddress.IsLoopback(address))
        {
            throw new ArgumentException("Serve host must be a loopback address.", nameof(listenAddress));
        }

        return new IPEndPoint(address, port);
    }
}
