using System.Data;

namespace Backend.Fx.Persistence.AdoNet;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}