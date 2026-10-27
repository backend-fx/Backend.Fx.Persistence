using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Execution.Pipeline;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Persistence.Feature;

[UsedImplicitly]
public class DbConnectionOperationDecorator : IOperation
{
    private readonly ILogger _logger = Log.Create<DbConnectionOperationDecorator>();
    private readonly IOperation _operation;
    private readonly IDbConnection _dbConnection;

    public DbConnectionOperationDecorator(IDbConnection dbConnection, IOperation operation)
    {
        _dbConnection = dbConnection;
        _operation = operation;
    }

    public async Task BeginAsync(
        IServiceScope serviceScope,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Opening database connection");
        _dbConnection.Open();
        await _operation.BeginAsync(serviceScope, cancellationToken).ConfigureAwait(false);
    }

    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        // Keep the connection open if completion fails: the invoker still needs to roll back.
        await _operation.CompleteAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Closing database connection");
        _dbConnection.Close();
    }

    public async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        var cleanupFailed = false;
        try
        {
            await _operation.CancelAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            cleanupFailed = true;
            throw;
        }
        finally
        {
            try
            {
                _logger.LogDebug("Closing database connection");
                _dbConnection.Close();
            }
            catch (Exception ex) when (cleanupFailed)
            {
                // Do not replace the rollback error with a secondary cleanup failure.
                _logger.LogError(
                    ex,
                    "Failed to close database connection after cancellation failed for operation {OperationCounter}",
                    Counter
                );
            }
        }

        // note: we do not dispose the DbConnection here, because we did not instantiate it. Disposing is always
        // up to the creator of the instance, that is in this case the injection container.
    }

    public int Counter => _operation.Counter;
}
