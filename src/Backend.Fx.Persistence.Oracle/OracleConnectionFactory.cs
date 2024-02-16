using System.Data;
using Backend.Fx.Persistence.AdoNet;
using JetBrains.Annotations;
using Oracle.ManagedDataAccess.Client;

namespace Backend.Fx.Persistence.Oracle;

[PublicAPI]
public class OracleConnectionFactory : IDbConnectionFactory
{
    private readonly OracleConnectionStringBuilder _connectionStringBuilder;

    public OracleConnectionFactory(OracleConnectionStringBuilder connectionStringBuilder)
    {
        _connectionStringBuilder = connectionStringBuilder;
    }

    public string ConnectionString => _connectionStringBuilder.ConnectionString;

    public IDbConnection Create()
    {
        return new OracleConnection(ConnectionString);
    }
}
