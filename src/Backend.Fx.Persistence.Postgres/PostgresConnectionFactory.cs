using System.Data;
using Backend.Fx.Persistence.AdoNet;
using JetBrains.Annotations;
using Npgsql;

namespace Backend.Fx.Persistence.Postgres;

[PublicAPI]
public class PostgresConnectionFactory : IDbConnectionFactory
{
    private readonly NpgsqlConnectionStringBuilder _connectionStringBuilder;

    public PostgresConnectionFactory(NpgsqlConnectionStringBuilder connectionStringBuilder)
    {
        _connectionStringBuilder = connectionStringBuilder;
    }

    public string ConnectionString => _connectionStringBuilder.ConnectionString;

    public IDbConnection Create()
    {
        return new NpgsqlConnection(ConnectionString);
    }
}