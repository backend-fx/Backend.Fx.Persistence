using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FakeItEasy;

namespace Backend.Fx.Persistence.Tests;

public class FakeDataSource : DbDataSource
{
    public IDbConnection ConnectionSpy { get; } = A.Fake<IDbConnection>();

    public IDbTransaction TransactionSpy { get; } = A.Fake<IDbTransaction>();

    protected override DbConnection CreateDbConnection()
    {
        return new FakeDbConnection(ConnectionSpy, TransactionSpy);
    }

    public override string ConnectionString => "Not=A;Connection=String;";


    private class FakeDbConnection(IDbConnection connectionSpy, IDbTransaction transactionSpy) : DbConnection
    {
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            connectionSpy.BeginTransaction(isolationLevel);
            return new FakeDbTransaction(this, transactionSpy);
        }

        public override void ChangeDatabase(string databaseName)
        {
            connectionSpy.ChangeDatabase(databaseName);
        }

        public override void Close()
        {
            connectionSpy.Close();
        }

        public override void Open()
        {
            connectionSpy.Open();
        }

        protected override void Dispose(bool disposing)
        {
            connectionSpy.Dispose();
            base.Dispose(disposing);
        }

        public override ValueTask DisposeAsync()
        {
            connectionSpy.Dispose();
            return base.DisposeAsync();
        }

        [AllowNull]
        public override string ConnectionString
        {
            get => connectionSpy.ConnectionString;
            set => connectionSpy.ConnectionString = value;
        }

        public override string Database => connectionSpy.Database;

        public override ConnectionState State => connectionSpy.State;

        public override string DataSource => "not-a-data-source";

        public override string ServerVersion => "not-a-server-version";

        protected override DbCommand CreateDbCommand()
        {
            throw new NotSupportedException();
        }
    }


    private class FakeDbTransaction(FakeDbConnection dbConnection, IDbTransaction transactionSpy) : DbTransaction
    {
        public override void Commit()
        {
            transactionSpy.Commit();
        }

        public override void Rollback()
        {
            transactionSpy.Rollback();
        }

        public override IsolationLevel IsolationLevel => transactionSpy.IsolationLevel;

        protected override DbConnection DbConnection => dbConnection;
    }
}
