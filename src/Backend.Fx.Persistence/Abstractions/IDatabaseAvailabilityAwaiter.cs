using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Backend.Fx.Persistence.Abstractions;

[PublicAPI]
public interface IDatabaseAvailabilityAwaiter
{
    void WaitForDatabase(Duration? timeout = null);

    Task WaitForDatabaseAsync(Duration? timeout = null, CancellationToken cancellationToken = default);
}

[PublicAPI]
public class TcpSocketAvailabilityAwaiter : IDatabaseAvailabilityAwaiter
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

    public void WaitForDatabase(Duration? timeout = null)
    {
        var timeoutElapsedOn = SystemClock.Instance.GetCurrentInstant().Plus(timeout ?? Duration.FromSeconds(60));
        do
        {
            try
            {
                using var client = new TcpClient();
                client.Connect(_hostname, _port);
                return;
            }
            catch (SocketException ex)
            {
                _logger.LogInformation(ex, "Database at {hostname}:{port} not healthy yet...", _hostname, _port);
                Thread.Sleep(1000);
            }
        } while (SystemClock.Instance.GetCurrentInstant() < timeoutElapsedOn);

        throw new Exception($"Database at {_hostname}:{_port} not healthy after {timeout}");
    }

    public async Task WaitForDatabaseAsync(Duration? timeout = null, CancellationToken cancellationToken = default)
    {
        var timeoutElapsedOn = SystemClock.Instance.GetCurrentInstant().Plus(timeout ?? Duration.FromSeconds(60));
        do
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(_hostname, _port);
                return;
            }
            catch (SocketException ex)
            {
                _logger.LogInformation(ex, "Database at {hostname}:{port} not healthy yet...", _hostname, _port);
                await Task.Delay(1000, cancellationToken);
            }
        } while (SystemClock.Instance.GetCurrentInstant() < timeoutElapsedOn);

        throw new Exception($"Database at {_hostname}:{_port} not healthy after {timeout}");
    }
}