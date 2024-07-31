using System;
using Backend.Fx.Execution;
using Backend.Fx.Execution.SimpleInjector;
using Backend.Fx.Logging;
using Backend.Fx.Persistence.Abstractions;
using Backend.Fx.Persistence.AdoNet;
using Backend.Fx.Persistence.Feature;
using Backend.Fx.Persistence.IdGeneration;
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

        persistenceFeature.AddIdGenerator<ThisIdGenerator, ThisId>();
        persistenceFeature.AddIdGenerator(new ThatIdGenerator());

        EnableFeature(persistenceFeature);
    }
}


public readonly record struct ThisId(Guid Value);


public class ThisIdGenerator : IIdGenerator<ThisId>
{
    public ThisId NextId()
    {
        return new ThisId(Guid.NewGuid());
    }
}


public readonly record struct ThatId(Guid Value);


public class ThatIdGenerator : IIdGenerator<ThatId>
{
    public ThatId NextId()
    {
        return new ThatId(Guid.NewGuid());
    }
}
