using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Execution;
using Backend.Fx.Execution.DependencyInjection;
using Backend.Fx.Execution.Features;
using Backend.Fx.Execution.Pipeline;
using Backend.Fx.Logging;
using Backend.Fx.Persistence.Abstractions;
using Backend.Fx.Persistence.AdoNet;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Persistence.Feature;

[PublicAPI]
public class PersistenceFeature : IFeature, IBootableFeature
{
    private static readonly ILogger Logger = Log.Create<PersistenceFeature>();
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly bool _enableTransactions;
    private readonly IDatabaseAvailabilityAwaiter _databaseAvailabilityAwaiter;
    private readonly IDatabaseBootstrapper _databaseBootstrapper;
    private readonly List<IModule> _idGenerationModules = [];

    /// <param name="dbConnectionFactory">
    /// The factory holds the connection string and creates <c>IDbConnection</c>s. Opening and closing of connections
    /// is done automatically for each <see cref="IOperation"/>. The connection is available as scoped service.
    /// </param>
    /// <param name="databaseAvailabilityAwaiter">
    /// An optional implementation of <see cref="IDatabaseAvailabilityAwaiter"/> that is used to wait for the database
    /// to become healthy. If none is provided, the database won't be awaited gracefully.
    /// </param>
    /// <param name="databaseBootstrapper">
    /// An optional implementation of <see cref="IDatabaseBootstrapper"/> that is used to create and/or migrate the
    /// database during application boot.
    /// </param>
    /// <param name="enableTransactions">
    /// Should each <see cref="IOperation"/> be wrapped in a transaction? Default: true.
    /// </param>
    public PersistenceFeature(
        IDbConnectionFactory dbConnectionFactory,
        IDatabaseAvailabilityAwaiter? databaseAvailabilityAwaiter = null,
        IDatabaseBootstrapper? databaseBootstrapper = null,
        bool enableTransactions = true)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _enableTransactions = enableTransactions;
        _databaseAvailabilityAwaiter = databaseAvailabilityAwaiter ?? new NullDatabaseAvailabilityAwaiter();
        _databaseBootstrapper = databaseBootstrapper ?? new NullDatabaseBootstrapper();
    }

    public void Enable(IBackendFxApplication application)
    {
        Logger.LogInformation("Enabling persistence for the {ApplicationName}", application.GetType().Name);
        application.CompositionRoot.RegisterModules(new PersistenceModule(_dbConnectionFactory, _enableTransactions));
        application.CompositionRoot.RegisterModules(_idGenerationModules.ToArray());
    }

    public async Task BootAsync(IBackendFxApplication application, CancellationToken cancellationToken = default)
    {
        await _databaseAvailabilityAwaiter.WaitForDatabaseAsync(cancellationToken);
        await _databaseBootstrapper.EnsureDatabaseExistenceAsync(cancellationToken);
    }
}
