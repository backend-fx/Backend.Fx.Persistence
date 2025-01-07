using System.Data;
using Backend.Fx.Logging;
using Backend.Fx.Persistence.Abstractions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Persistence.AdoNet;

[PublicAPI]
public abstract class AdoNetDbUtil(IDbConnectionFactory dbConnectionFactory) : IDbUtil
{
    private static readonly ILogger Logger = Log.Create<AdoNetDbUtil>();

    public string ConnectionString => dbConnectionFactory.ConnectionString;

    public virtual void EnsureDroppedDatabase(string dbName)
    {
        using var connection = dbConnectionFactory.Create();
        connection.Open();

        bool exists = ExistsDatabase(dbName);

        if (exists)
        {
            Logger.LogInformation($"Dropping database {dbName}...");
            DropDatabase(dbName, connection);
        }
    }

    public virtual bool ExistsDatabase(string dbName)
    {
        bool exists;
        using (var connection = dbConnectionFactory.Create())
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

        using (var connection = dbConnectionFactory.Create())
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
        using IDbConnection connection = dbConnectionFactory.Create();
        connection.Open();

        using var createCommand = connection.CreateCommand();
        createCommand.CommandText = GetCreateDatabaseCommand(dbName);
        createCommand.ExecuteNonQuery();
    }

    public virtual void CreateSchema(string schemaName, string? createScriptContent = null)
    {
        Logger.LogInformation($"Creating schema {schemaName}...");
        using var connection = dbConnectionFactory.Create();
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
}
