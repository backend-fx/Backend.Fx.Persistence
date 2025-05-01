using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Persistence.Abstractions;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Persistence.Tests;

public class ThePersistenceFeature
{
    private readonly IDatabaseAvailabilityAwaiter _databaseAvailabilityAwaiter = A.Fake<IDatabaseAvailabilityAwaiter>();
    private readonly IDatabaseBootstrapper _databaseBootstrapper = A.Fake<IDatabaseBootstrapper>();
    private readonly TestApplication _app;
    private readonly FakeDataSource _fakeDataSource = new();

    public ThePersistenceFeature()
    {
        _app = new TestApplication(_fakeDataSource, _databaseAvailabilityAwaiter, _databaseBootstrapper);
    }

    [Fact]
    public async Task DatabaseAwaiterIsOptional()
    {
        var app = new TestApplication(_fakeDataSource, databaseBootstrapper: _databaseBootstrapper);
        await app.BootAsync();
    }

    [Fact]
    public async Task DatabaseBootstrapperIsOptional()
    {
        var app = new TestApplication(_fakeDataSource, databaseAvailabilityAwaiter: _databaseAvailabilityAwaiter);
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
    public async Task DoesNotOpenAnyConnectionsOnBoot()
    {
        await _app.BootAsync();

        A.CallTo(() => _fakeDataSource.ConnectionSpy.Open()).MustNotHaveHappened();
    }

    [Fact]
    public async Task MaintainsConnectionAndTransactionOnOperations()
    {
        var whatever = A.Fake<IFormattable>();

        await _app.BootAsync();
        await _app.Invoker.InvokeAsync((_, _) =>
                                       {
                                           _ = whatever.ToString();
                                           return Task.CompletedTask;
                                       });

        A.CallTo(() => _fakeDataSource.ConnectionSpy.Open())
            .MustHaveHappenedOnceExactly()
            .Then(A.CallTo(() => _fakeDataSource.ConnectionSpy.BeginTransaction(A<IsolationLevel>._)).MustHaveHappenedOnceExactly())
            .Then(A.CallTo(() => whatever.ToString()).MustHaveHappenedOnceExactly())
            .Then(A.CallTo(() => _fakeDataSource.TransactionSpy.Commit()).MustHaveHappenedOnceExactly())
            .Then(A.CallTo(() => _fakeDataSource.ConnectionSpy.Close()).MustHaveHappenedOnceExactly())
            .Then(A.CallTo(() => _fakeDataSource.ConnectionSpy.Dispose()).MustHaveHappened());

        A.CallTo(() => _fakeDataSource.TransactionSpy.Rollback()).MustNotHaveHappened();
    }

    [Fact]
    public async Task MaintainsConnectionAndTransactionOnFailingOperations()
    {
        await _app.BootAsync();
        await Assert.ThrowsAsync<DivideByZeroException>(async () =>
                                                            await _app.Invoker.InvokeAsync((_, _) =>
                                                                throw new DivideByZeroException()));

        A.CallTo(() => _fakeDataSource.ConnectionSpy.Open())
            .MustHaveHappenedOnceExactly()
            .Then(A.CallTo(() => _fakeDataSource.ConnectionSpy.BeginTransaction(A<IsolationLevel>._)).MustHaveHappenedOnceExactly())
            .Then(A.CallTo(() => _fakeDataSource.TransactionSpy.Rollback()).MustHaveHappenedOnceExactly())
            .Then(A.CallTo(() => _fakeDataSource.ConnectionSpy.Close()).MustHaveHappenedOnceExactly())
            .Then(A.CallTo(() => _fakeDataSource.ConnectionSpy.Dispose()).MustHaveHappened());

        A.CallTo(() => _fakeDataSource.TransactionSpy.Commit()).MustNotHaveHappened();
    }

    [Fact]
    public async Task AllowsDisablingTransactions()
    {
        var whatever = A.Fake<IFormattable>();
        var app = new TestApplication(
            _fakeDataSource, _databaseAvailabilityAwaiter, _databaseBootstrapper, enableTransactions: false);
        await app.BootAsync();

        await app.Invoker.InvokeAsync((_, _) =>
                                      {
                                          _ = whatever.ToString();
                                          return Task.CompletedTask;
                                      });

        A.CallTo(() => _fakeDataSource.ConnectionSpy.Open())
            .MustHaveHappenedOnceExactly()
            .Then(A.CallTo(() => whatever.ToString()).MustHaveHappenedOnceExactly())
            .Then(A.CallTo(() => _fakeDataSource.ConnectionSpy.Close()).MustHaveHappenedOnceExactly())
            .Then(A.CallTo(() => _fakeDataSource.ConnectionSpy.Dispose()).MustHaveHappened());

        A.CallTo(() => _fakeDataSource.ConnectionSpy.BeginTransaction(A<IsolationLevel>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeDataSource.TransactionSpy.Commit()).MustNotHaveHappened();
        A.CallTo(() => _fakeDataSource.TransactionSpy.Rollback()).MustNotHaveHappened();
    }

    [Fact]
    public async Task AllowsDisablingTransactionsWithFailingCall()
    {
        var app = new TestApplication(
            _fakeDataSource, _databaseAvailabilityAwaiter, _databaseBootstrapper, enableTransactions: false);
        await app.BootAsync();

        await Assert.ThrowsAsync<DivideByZeroException>(async () =>
                                                            await app.Invoker.InvokeAsync((_, _) =>
                                                                throw new DivideByZeroException()));

        A.CallTo(() => _fakeDataSource.ConnectionSpy.Open())
            .MustHaveHappenedOnceExactly()
            .Then(A.CallTo(() => _fakeDataSource.ConnectionSpy.Close()).MustHaveHappenedOnceExactly())
            .Then(A.CallTo(() => _fakeDataSource.ConnectionSpy.Dispose()).MustHaveHappened());

        A.CallTo(() => _fakeDataSource.ConnectionSpy.BeginTransaction(A<IsolationLevel>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeDataSource.TransactionSpy.Commit()).MustNotHaveHappened();
        A.CallTo(() => _fakeDataSource.TransactionSpy.Rollback()).MustNotHaveHappened();
    }
}
