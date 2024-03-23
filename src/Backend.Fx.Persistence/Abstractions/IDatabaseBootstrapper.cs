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

    public DatabaseState State { get; }


    public enum DatabaseState
    {
        NotAvailableYet,
        BootStrapping,
        BootstrappingFailed,
        Ready,
    }
}

[PublicAPI]
public abstract class DatabaseBootstrapper(IDatabaseAvailabilityAwaiter databaseAvailabilityAwaiter)
    : IDatabaseBootstrapper
{
    public IDatabaseAvailabilityAwaiter DatabaseAvailabilityAwaiter { get; } = databaseAvailabilityAwaiter;

    public IDatabaseBootstrapper.DatabaseState State { get; private set; } =
        IDatabaseBootstrapper.DatabaseState.NotAvailableYet;

    public async Task EnsureDatabaseExistenceAsync(CancellationToken cancellationToken)
    {
        await DatabaseAvailabilityAwaiter.WaitForDatabaseAsync(cancellationToken);
        State = IDatabaseBootstrapper.DatabaseState.BootStrapping;

        try
        {
            await EnsureDatabaseExistenceWhenDatabaseIsAvailableAsync(cancellationToken);
            State = IDatabaseBootstrapper.DatabaseState.Ready;
        }
        catch
        {
            State = IDatabaseBootstrapper.DatabaseState.BootstrappingFailed;
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
}