using System.Data;
using Backend.Fx.Persistence.AdoNet;
using JetBrains.Annotations;
using Npgsql;

namespace Backend.Fx.Persistence.Postgres;

[PublicAPI]
public class PostgresConnectionFactory : IDbConnectionFactory
{
    private readonly NpgsqlConnectionStringBuilder _connectionStringBuilder;
    private readonly NpgsqlDataSource _dataSource;

    public PostgresConnectionFactory(NpgsqlConnectionStringBuilder connectionStringBuilder)
    {
        _connectionStringBuilder = connectionStringBuilder;
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionStringBuilder.ConnectionString);
        dataSourceBuilder.UseNodaTime();
        _dataSource = dataSourceBuilder.Build();
    }

    public string ConnectionString => _connectionStringBuilder.ConnectionString;

    public IDbConnection Create()
    {
        return _dataSource.OpenConnection();
    }
}
