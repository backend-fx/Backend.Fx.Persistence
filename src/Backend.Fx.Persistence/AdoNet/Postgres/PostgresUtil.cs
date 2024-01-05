using System.Data;
using JetBrains.Annotations;

namespace Backend.Fx.Persistence.AdoNet.Postgres;

[PublicAPI]
public class PostgresUtil : DbUtil
{
    public PostgresUtil(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    { }

    protected override string GetIsAvailableCheckCommand() => "SELECT 1";

    protected override string GetCreateSchemaCommand(string schemaName) => $"CREATE SCHEMA \"{schemaName}\"";

    protected override string GetExistsDatabaseCommand(string dbName) =>
        $"SELECT 1 FROM pg_database WHERE datname = '{dbName}'";

    protected override void DropDatabase(string dbName, IDbConnection connection)
    {
        using (var singleUserCmd = connection.CreateCommand())
        {
            singleUserCmd.CommandText = $"UPDATE pg_database SET datallowconn = 'false' WHERE datname = '{dbName}';";
            singleUserCmd.ExecuteNonQuery();
        }

        using (var termConnCmd = connection.CreateCommand())
        {
            termConnCmd.CommandText =
                $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{dbName}';";
            termConnCmd.ExecuteNonQuery();
        }

        using (var dropDbCmd = connection.CreateCommand())
        {
            dropDbCmd.CommandText = $"DROP DATABASE \"{dbName}\";";
            dropDbCmd.ExecuteNonQuery();
        }
    }

    protected override string GetExistsTableCommand(string schemaName, string tableName) =>
        $"SELECT 1 FROM information_schema.tables WHERE table_name = '{tableName}' AND table_schema = '{schemaName}'";

    protected override string GetCreateDatabaseCommand(string dbName) => $"CREATE DATABASE \"{dbName}\"";
}
