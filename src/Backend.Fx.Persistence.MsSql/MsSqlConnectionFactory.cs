using System;
using System.Data;
using Backend.Fx.Persistence.AdoNet;
using JetBrains.Annotations;
using Microsoft.Data.SqlClient;

namespace Backend.Fx.Persistence.MsSql;

[PublicAPI]
[Obsolete]
public class MsSqlConnectionFactory : IDbConnectionFactory
{
    private readonly SqlConnectionStringBuilder _connectionStringBuilder;

    public MsSqlConnectionFactory(SqlConnectionStringBuilder connectionStringBuilder)
    {
        _connectionStringBuilder = connectionStringBuilder;
    }

    public string ConnectionString => _connectionStringBuilder.ConnectionString;

    public IDbConnection Create()
    {
        return new SqlConnection(ConnectionString);
    }
}
