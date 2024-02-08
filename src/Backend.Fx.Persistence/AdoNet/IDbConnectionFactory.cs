using System.Data;

namespace Backend.Fx.Persistence.AdoNet;

public interface IDbConnectionFactory
{
    public string ConnectionString { get; }
    
    IDbConnection Create();
}