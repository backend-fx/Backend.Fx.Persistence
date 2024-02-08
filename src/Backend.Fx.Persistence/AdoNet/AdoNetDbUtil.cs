using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using Backend.Fx.Persistence.Abstractions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Backend.Fx.Persistence.AdoNet;

[PublicAPI]
public abstract class AdoNetDbUtil : IDbUtil
{
    private static readonly ILogger Logger = Log.Create<AdoNetDbUtil>();
    private readonly IDbConnectionFactory _dbConnectionFactory;

    protected AdoNetDbUtil(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public string ConnectionString => _dbConnectionFactory.ConnectionString;

    public bool WaitUntilAvailable() => WaitUntilAvailable(Duration.FromDays(1));

    public bool WaitUntilAvailable(Duration timeout)
    {
        Logger.LogInformation("Probing for Postgres instance with {timeout}.", timeout);

        var waitUntil = SystemClock.Instance.GetCurrentInstant().Plus(timeout);

        while (!IsAvailable())
        {
            if (SystemClock.Instance.GetCurrentInstant() > waitUntil)
            {
                return false;
            }

            Thread.Sleep(1000);
        }

        return true;
    }

    public Task<bool> WaitUntilAvailableAsync(CancellationToken cancellationToken) =>
        WaitUntilAvailableAsync(Duration.FromDays(1), cancellationToken);

    public async Task<bool> WaitUntilAvailableAsync(Duration timeout, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Probing for Postgres instance with {timeout}.", timeout);

        Instant waitUntil = SystemClock.Instance.GetCurrentInstant().Plus(timeout);

        while (!IsAvailable())
        {
            if (SystemClock.Instance.GetCurrentInstant() > waitUntil)
            {
                return false;
            }

            await Task.Delay(1000, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
        }

        return true;
    }

    public void EnsureDroppedDatabase(string dbName)
    {
        using var connection = _dbConnectionFactory.Create();
        connection.Open();

        bool exists = ExistsDatabase(dbName);

        if (exists)
        {
            Logger.LogInformation($"Dropping database {dbName}...");
            DropDatabase(dbName, connection);
        }

        //todo? NpgsqlConnection.ClearAllPools();
    }

    public virtual bool ExistsDatabase(string dbName)
    {
        bool exists;
        using (var connection = _dbConnectionFactory.Create())
        {
            connection.Open();
            using (var existsCommand = connection.CreateCommand())
            {
                existsCommand.CommandText = GetExistsDatabaseCommand(dbName);
                exists = (int?)existsCommand.ExecuteScalar() == 1;
            }
        }

        Logger.LogInformation($"Database {dbName} {(exists ? "exists" : "does not exist")}");

        return exists;
    }


    public virtual bool ExistsTable(string schemaName, string tableName)
    {
        bool exists;

        using (var connection = _dbConnectionFactory.Create())
        {
            connection.Open();
            using (var existsCommand = connection.CreateCommand())
            {
                existsCommand.CommandText = GetExistsTableCommand(schemaName, tableName);
                exists = (int?)existsCommand.ExecuteScalar() == 1;
            }
        }

        Logger.LogInformation($"Table {schemaName}.{tableName} {(exists ? "exists" : "does not exist")}");

        return exists;
    }


    public virtual void CreateDatabase(string dbName)
    {
        Logger.LogInformation($"Creating database {dbName}...");
        using IDbConnection connection = _dbConnectionFactory.Create();
        connection.Open();

        using var createCommand = connection.CreateCommand();
        createCommand.CommandText = GetCreateDatabaseCommand(dbName);
        createCommand.ExecuteNonQuery();
    }

    public virtual void CreateSchema(string schemaName, string? createScriptContent = null)
    {
        Logger.LogInformation($"Creating schema {schemaName}...");
        using var connection = _dbConnectionFactory.Create();
        connection.Open();

        using (var createCommand = connection.CreateCommand())
        {
            createCommand.CommandText = GetCreateSchemaCommand(schemaName);
            createCommand.ExecuteNonQuery();
        }

        if (!string.IsNullOrEmpty(createScriptContent))
        {
            using var createCommand = connection.CreateCommand();
            createCommand.CommandText = createScriptContent;
            createCommand.ExecuteNonQuery();
        }
    }

    protected abstract string GetIsAvailableCheckCommand();

    protected abstract string GetCreateSchemaCommand(string schemaName);

    protected abstract string GetExistsDatabaseCommand(string dbName);
    
    protected abstract void DropDatabase(string dbName, IDbConnection connection);
    
    protected abstract string GetExistsTableCommand(string schemaName, string tableName);
    
    protected abstract string GetCreateDatabaseCommand(string dbName);

    private bool IsAvailable()
    {
        try
        {
            using var dbConnection = _dbConnectionFactory.Create();
            dbConnection.Open();
            using var command = dbConnection.CreateCommand();
            command.CommandText = GetIsAvailableCheckCommand();
            command.ExecuteScalar();
            return true;
        }
        catch (DataException ex)
        {
            Logger.LogInformation(ex, "Database not healthy yet...");
            return false;
        }
    }
}