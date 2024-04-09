using System;
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

    Task WaitForDatabaseAsync(CancellationToken cancellationToken = default);
}

public abstract class DatabaseAvailabilityAwaiter : IDatabaseAvailabilityAwaiter
{
    private readonly ILogger _logger = Log.Create<DatabaseAvailabilityAwaiter>();

    public void WaitForDatabase(Duration? timeout = null)
    {
        var timeoutEnd = SystemClock.Instance.GetCurrentInstant().Plus(timeout ?? Duration.FromDays(1));

        _logger.LogInformation("Waiting for database to become available");

        while (true)
        {
            try
            {
                ConnectToDatabase();
                _logger.LogInformation("Database is available");
                return;
            }
            catch (Exception ex)
            {
                if (timeoutEnd < SystemClock.Instance.GetCurrentInstant())
                {
                    _logger.LogError($@"Database not yet ready ({ex.Message}) - aborting");
                    throw;
                }

                _logger.LogInformation($@"Database not yet ready ({ex.Message}) - retrying...");
                Thread.Sleep(3000);
            }
        }
    }


    public async Task WaitForDatabaseAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Waiting for database to become available");

        while (true)
        {
            try
            {
                await ConnectToDatabaseAsync(cancellationToken);
                _logger.LogInformation("Database is available");
                return;
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError($"Database not yet ready ({ex.Message}) - aborting");
                    throw;
                }

                _logger.LogInformation($"Database not yet ready ({ex.Message}) - retrying...");
                await Task.Delay(3000, cancellationToken);
            }
        }
    }

    protected abstract void ConnectToDatabase();
    
    protected abstract Task ConnectToDatabaseAsync(CancellationToken cancellationToken);
}