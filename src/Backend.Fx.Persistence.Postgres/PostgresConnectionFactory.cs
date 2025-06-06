using System;
using System.Data;
using Backend.Fx.Persistence.AdoNet;
using JetBrains.Annotations;
using Npgsql;

namespace Backend.Fx.Persistence.Postgres;

[PublicAPI]
[Obsolete]
public class PostgresConnectionFactory : IDbConnectionFactory
{
    private readonly NpgsqlConnectionStringBuilder _connectionStringBuilder;
    private readonly NpgsqlDataSource _dataSource;

    public PostgresConnectionFactory(
        NpgsqlConnectionStringBuilder connectionStringBuilder,
        Action<NpgsqlDataSourceBuilder>? configure = null)
    {
        _connectionStringBuilder = connectionStringBuilder;
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionStringBuilder.ConnectionString);
        configure?.Invoke(dataSourceBuilder);
        _dataSource = dataSourceBuilder.Build();
    }

    public virtual void ConfigureDataSource(NpgsqlDataSourceBuilder dataSourceBuilder)
    { }

    public string ConnectionString => _connectionStringBuilder.ConnectionString;

    public IDbConnection Create()
    {
        return _dataSource.CreateConnection();
    }
}
