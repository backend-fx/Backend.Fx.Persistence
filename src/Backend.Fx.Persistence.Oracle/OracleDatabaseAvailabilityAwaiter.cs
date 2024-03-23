using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Persistence.Abstractions;
using JetBrains.Annotations;
using Oracle.ManagedDataAccess.Client;

namespace Backend.Fx.Persistence.Oracle;

[PublicAPI]
public class OracleDatabaseAvailabilityAwaiter(string connectionString) : DatabaseAvailabilityAwaiter
{
    protected override void ConnectToDatabase()
    {
        using var connection = new OracleConnection(connectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandTimeout = 1;
        cmd.CommandText = "select 1 from DUAL";
        cmd.ExecuteScalar();
    }

    protected override async Task ConnectToDatabaseAsync(CancellationToken cancellationToken)
    {
        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = connection.CreateCommand();
        cmd.CommandTimeout = 1;
        cmd.CommandText = "select 1 from DUAL";
        await cmd.ExecuteScalarAsync(cancellationToken);
    }
}