using Backend.Fx.Execution;
using Backend.Fx.Execution.SimpleInjector;
using Backend.Fx.Logging;
using Backend.Fx.Persistence.Abstractions;
using Backend.Fx.Persistence.AdoNet;
using Backend.Fx.Persistence.Feature;
using FakeItEasy;

namespace Backend.Fx.Persistence.Tests;

public sealed class TestApplication : BackendFxApplication
{
    public TestApplication(
        IDbConnectionFactory dbConnectionFactory,
        IDatabaseAvailabilityAwaiter? databaseAvailabilityAwaiter = null,
        IDatabaseBootstrapper? databaseBootstrapper = null,
        bool enableTransactions = true)
        : base(new SimpleInjectorCompositionRoot(), A.Fake<IExceptionLogger>(), typeof(TestApplication).Assembly)
    {
        var persistenceFeature = new PersistenceFeature(
            dbConnectionFactory, databaseAvailabilityAwaiter, databaseBootstrapper, enableTransactions);

        EnableFeature(persistenceFeature);
    }
}
