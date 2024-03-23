using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Backend.Fx.Persistence.Abstractions;

/// <summary>
/// Encapsulates database bootstrapping. This interface hides the implementation details for creating/migrating the database
/// </summary>
[PublicAPI]
public interface IDatabaseBootstrapper : IDisposable
{
    Task EnsureDatabaseExistenceAsync(CancellationToken cancellationToken);
}

[PublicAPI]
public abstract class DatabaseBootstrapper(IDatabaseAvailabilityAwaiter databaseAvailabilityAwaiter)
    : IDatabaseBootstrapper
{
    public IDatabaseAvailabilityAwaiter DatabaseAvailabilityAwaiter { get; } = databaseAvailabilityAwaiter;

    public DatabaseState State { get; private set; } = DatabaseState.NotAvailableYet;

    public async Task EnsureDatabaseExistenceAsync(CancellationToken cancellationToken)
    {
        await DatabaseAvailabilityAwaiter.WaitForDatabaseAsync(cancellationToken);
        State = DatabaseState.BootStrapping;

        try
        {
            await EnsureDatabaseExistenceWhenDatabaseIsAvailableAsync(cancellationToken);
            State = DatabaseState.Ready;
        }
        catch
        {
            State = DatabaseState.BootstrappingFailed;
            throw;
        }
    }

    protected abstract Task EnsureDatabaseExistenceWhenDatabaseIsAvailableAsync(CancellationToken cancellationToken);

    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    public enum DatabaseState
    {
        NotAvailableYet,
        BootStrapping,
        BootstrappingFailed,
        Ready,
    }
}