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