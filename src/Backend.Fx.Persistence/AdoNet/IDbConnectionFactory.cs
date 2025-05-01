using System;
using System.Data;

namespace Backend.Fx.Persistence.AdoNet;

[Obsolete("Use DbDataSource instead.")]
public interface IDbConnectionFactory
{
    public string ConnectionString { get; }

    IDbConnection Create();
}
