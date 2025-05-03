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
    private IDisposable? _transactionLifetimeLogger;
    private TxState _state = TxState.NotStarted;

    public DbTransactionOperationDecorator(
        IDbConnection dbConnection,
        ICurrentTHolder<IDbTransaction?> currentTransactionHolder,
        IOperation operation)
    {
        _dbConnection = dbConnection;
        _currentTransactionHolder = currentTransactionHolder;
        _operation = operation;
    }

    public Task BeginAsync(IServiceScope serviceScope, CancellationToken cancellationToken = default)
    {
        if (_state != TxState.NotStarted)
        {
            throw new InvalidOperationException("A Transaction has been started by this operation before.");
        }

        _logger.LogDebug("Beginning transaction");
        _currentTransactionHolder.ReplaceCurrent(_dbConnection.BeginTransaction());
        _transactionLifetimeLogger = _logger.LogDebugDuration("Transaction open", "Transaction terminated");
        _state = TxState.Active;
        return _operation.BeginAsync(serviceScope, cancellationToken);
    }

    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        await _operation.CompleteAsync(cancellationToken).ConfigureAwait(false);

        if (_state != TxState.Active)
        {
            throw new InvalidOperationException($"A transaction cannot be committed when it is {_state}.");
        }

        if (_currentTransactionHolder.Current == null)
        {
            throw new InvalidOperationException("No current transaction to complete.");
        }

        _logger.LogDebug("Committing transaction");
        _currentTransactionHolder.Current.Commit();
        _currentTransactionHolder.Current.Dispose();
        _currentTransactionHolder.ReplaceCurrent(null);
        _transactionLifetimeLogger?.Dispose();
        _transactionLifetimeLogger = null;

        _state = TxState.Committed;
    }

    public async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("rolling back transaction");
        if (_state == TxState.Committed || _state == TxState.RolledBack)
        {
            throw new InvalidOperationException($"Cannot roll back a transaction that is {_state}");
        }

        await _operation.CancelAsync(cancellationToken).ConfigureAwait(false);

        if (_state == TxState.Active)
        {
            _currentTransactionHolder.Current?.Rollback();
        }

        _currentTransactionHolder.ClearCurrent();

        _transactionLifetimeLogger?.Dispose();
        _transactionLifetimeLogger = null;

        _state = TxState.RolledBack;
    }


    private enum TxState
    {
        NotStarted,
        Active,
        Committed,
        RolledBack
    }
}
