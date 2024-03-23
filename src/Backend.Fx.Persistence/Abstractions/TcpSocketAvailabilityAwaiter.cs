using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Persistence.Abstractions;

[PublicAPI]
public class TcpSocketAvailabilityAwaiter : DatabaseAvailabilityAwaiter
{
    private readonly ILogger _logger = Log.Create<TcpSocketAvailabilityAwaiter>();
    private readonly string _hostname;
    private readonly int _port;

    public TcpSocketAvailabilityAwaiter(string hostname, int port)
    {
        _hostname = hostname ?? throw new ArgumentNullException(nameof(hostname));
        _port = port;

        if (_port <= 0)
        {
            throw new ArgumentException("Port must be greater than 0", nameof(port));
        }
    }

    protected override void ConnectToDatabase()
    {
        using var client = new TcpClient();
        client.Connect(_hostname, _port);
    }

    protected override async Task ConnectToDatabaseAsync(CancellationToken cancellationToken)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(_hostname, _port);
    }
}