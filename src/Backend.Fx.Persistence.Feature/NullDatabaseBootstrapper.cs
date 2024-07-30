using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Persistence.Abstractions;

namespace Backend.Fx.Persistence.Feature;

public class NullDatabaseBootstrapper : IDatabaseBootstrapper
{
    public void Dispose()
    { }

    public Task EnsureDatabaseExistenceAsync(CancellationToken cancellationToken)
    {
        State = IDatabaseBootstrapper.DatabaseState.Ready;
        return Task.CompletedTask;
    }

    public IDatabaseBootstrapper.DatabaseState State { get; private set; }
        = IDatabaseBootstrapper.DatabaseState.NotAvailableYet;
}
