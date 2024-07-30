using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Persistence.Abstractions;
using Backend.Fx.Persistence.AdoNet;
using Backend.Fx.Persistence.IdGeneration;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Backend.Fx.Persistence.Tests;

public class ThePersistenceFeature
{
    private readonly IDbConnection _dbConnection = A.Fake<IDbConnection>();
    private readonly IDbTransaction _dbTransaction = A.Fake<IDbTransaction>();
    private readonly IDbConnectionFactory _dbConnectionFactory = A.Fake<IDbConnectionFactory>();
    private readonly IDatabaseAvailabilityAwaiter _databaseAvailabilityAwaiter = A.Fake<IDatabaseAvailabilityAwaiter>();
    private readonly IDatabaseBootstrapper _databaseBootstrapper = A.Fake<IDatabaseBootstrapper>();
    private readonly TestApplication _app;

    public ThePersistenceFeature()
    {
        A.CallTo(() => _dbConnectionFactory.Create()).ReturnsLazily(() => _dbConnection);
        A.CallTo(() => _dbConnection.BeginTransaction(A<IsolationLevel>._)).ReturnsLazily(() => _dbTransaction);

        _app = new TestApplication(_dbConnectionFactory, _databaseAvailabilityAwaiter, _databaseBootstrapper);
    }

    [Fact]
    public async Task DatabaseAwaiterIsOptional()
    {
        var app = new TestApplication(_dbConnectionFactory, databaseBootstrapper: _databaseBootstrapper);
        await app.BootAsync();
    }

    [Fact]
    public async Task DatabaseBootstrapperIsOptional()
    {
        var app = new TestApplication(_dbConnectionFactory, databaseAvailabilityAwaiter: _databaseAvailabilityAwaiter);
        await app.BootAsync();
    }

    [Fact]
    public async Task RunsDatabaseAwaiterAndBootstrapperOnBoot()
    {
        await _app.BootAsync();

        A.CallTo(() => _databaseAvailabilityAwaiter.WaitForDatabaseAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _databaseBootstrapper.EnsureDatabaseExistenceAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CanAddIdGenerators()
    {
        await _app.BootAsync();

        var thisIdGenerator = _app.CompositionRoot.ServiceProvider.GetRequiredService<IIdGenerator<ThisId>>();
        Assert.IsType<ThisIdGenerator>(thisIdGenerator);

        var thatIdGenerator = _app.CompositionRoot.ServiceProvider.GetRequiredService<IIdGenerator<ThatId>>();
        Assert.IsType<ThatIdGenerator>(thatIdGenerator);
    }

    [Fact]
    public async Task DoesNotOpenAnyConnectionsOnBoot()
    {
        await _app.BootAsync();

        A.CallTo(() => _dbConnection.Open()).MustNotHaveHappened();
    }

    [Fact]
    public async Task MaintainsConnectionAndTransactionOnOperations()
    {
        await _app.BootAsync();
        await _app.Invoker.InvokeAsync((_, _) => Task.CompletedTask);

        A.CallTo(() => _dbConnection.Open()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dbConnection.BeginTransaction(A<IsolationLevel>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dbTransaction.Commit()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dbTransaction.Rollback()).MustNotHaveHappened();
        A.CallTo(() => _dbConnection.Close()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dbConnection.Dispose()).MustHaveHappened();
    }

    [Fact]
    public async Task MaintainsConnectionAndTransactionOnFailingOperations()
    {
        await _app.BootAsync();
        await Assert.ThrowsAsync<DivideByZeroException>(
            async () => await _app.Invoker.InvokeAsync((_, _) => throw new DivideByZeroException()));

        A.CallTo(() => _dbConnection.Open()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dbConnection.BeginTransaction(A<IsolationLevel>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dbTransaction.Commit()).MustNotHaveHappened();
        A.CallTo(() => _dbTransaction.Rollback()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dbConnection.Close()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dbConnection.Dispose()).MustHaveHappened();
    }

    [Fact]
    public async Task AllowsDisablingTransactions()
    {
        var app = new TestApplication(
            _dbConnectionFactory, _databaseAvailabilityAwaiter, _databaseBootstrapper, enableTransactions: false);
        await app.BootAsync();

        await app.Invoker.InvokeAsync((_, _) => Task.CompletedTask);

        A.CallTo(() => _dbConnection.Open()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dbConnection.Close()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dbConnection.Dispose()).MustHaveHappened();

        A.CallTo(() => _dbConnection.BeginTransaction(A<IsolationLevel>._)).MustNotHaveHappened();
        A.CallTo(() => _dbTransaction.Commit()).MustNotHaveHappened();
        A.CallTo(() => _dbTransaction.Rollback()).MustNotHaveHappened();
    }

    [Fact]
    public async Task AllowsDisablingTransactionsWithFailingCall()
    {
        var app = new TestApplication(
            _dbConnectionFactory, _databaseAvailabilityAwaiter, _databaseBootstrapper, enableTransactions: false);
        await app.BootAsync();

        await Assert.ThrowsAsync<DivideByZeroException>(
            async () => await app.Invoker.InvokeAsync((_, _) => throw new DivideByZeroException()));

        A.CallTo(() => _dbConnection.Open()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dbConnection.Close()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dbConnection.Dispose()).MustHaveHappened();

        A.CallTo(() => _dbConnection.BeginTransaction(A<IsolationLevel>._)).MustNotHaveHappened();
        A.CallTo(() => _dbTransaction.Commit()).MustNotHaveHappened();
        A.CallTo(() => _dbTransaction.Rollback()).MustNotHaveHappened();
    }
}
