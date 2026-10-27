using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Execution.Pipeline;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Persistence.Feature;

/// <summary>
/// Enriches the operation to use a database transaction during lifetime. The transaction gets started, before IOperation.Begin()
/// is being called and gets committed after IOperation.Complete() is being called.
/// </summary>
public class DbTransactionOperationDecorator : IOperation
{
    private readonly ILogger _logger = Log.Create<DbTransactionOperationDecorator>();
    private readonly IDbConnection _dbConnection;
    private readonly ICurrentTHolder<IDbTransaction?> _currentTransactionHolder;
    private readonly IOperation _operation;
    private TxState _state = TxState.NotStarted;
    private IDbTransaction? _currentTransaction;

    public DbTransactionOperationDecorator(
        IDbConnection dbConnection,
        ICurrentTHolder<IDbTransaction?> currentTransactionHolder,
        IOperation operation
    )
    {
        _dbConnection = dbConnection;
        _currentTransactionHolder = currentTransactionHolder;
        _operation = operation;
    }

    public async Task BeginAsync(
        IServiceScope serviceScope,
        CancellationToken cancellationToken = default
    )
    {
        if (_state != TxState.NotStarted)
        {
            throw new InvalidOperationException(
                "A Transaction has been started by this operation before."
            );
        }

        _logger.LogDebug("Beginning transaction");
        _currentTransaction = _dbConnection.BeginTransaction();
        _currentTransactionHolder.ReplaceCurrent(_currentTransaction);
        _state = TxState.Active;
        await _operation.BeginAsync(serviceScope, cancellationToken).ConfigureAwait(false);
    }

    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        await _operation.CompleteAsync(cancellationToken).ConfigureAwait(false);

        if (_state != TxState.Active)
        {
            throw new InvalidOperationException(
                $"A transaction cannot be committed when it is {_state}."
            );
        }

        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No current transaction to complete.");
        }

        _logger.LogDebug("Committing transaction");
        _currentTransaction.Commit();
        _state = TxState.Committed;

        try
        {
            _currentTransaction.Dispose();
        }
        catch (Exception ex)
        {
            // The commit succeeded, so a failing disposal must not fail the operation.
            _logger.LogError(
                ex,
                "Failed to dispose the committed transaction of operation {OperationCounter}",
                Counter
            );
        }
        finally
        {
            _currentTransaction = null;
            _currentTransactionHolder.ReplaceCurrent(null);
        }
    }

    public async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("rolling back transaction");
        if (_state == TxState.Committed || _state == TxState.RolledBack)
        {
            throw new InvalidOperationException($"Cannot roll back a transaction that is {_state}");
        }

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
                RollbackTransaction();
            }
            catch (Exception ex) when (cleanupFailed)
            {
                // Do not replace the cancellation error with a secondary rollback failure.
                _logger.LogError(
                    ex,
                    "Failed to roll back the transaction after cancellation failed for operation {OperationCounter}",
                    Counter
                );
            }
        }
    }

    private void RollbackTransaction()
    {
        try
        {
            if (_state == TxState.Active)
            {
                _currentTransaction?.Rollback();
            }
        }
        finally
        {
            try
            {
                _currentTransaction?.Dispose();
            }
            finally
            {
                _currentTransaction = null;
                _currentTransactionHolder.ClearCurrent();
                _state = TxState.RolledBack;
            }
        }
    }

    public int Counter => _operation.Counter;

    private enum TxState
    {
        NotStarted,
        Active,
        Committed,
        RolledBack,
    }
}
