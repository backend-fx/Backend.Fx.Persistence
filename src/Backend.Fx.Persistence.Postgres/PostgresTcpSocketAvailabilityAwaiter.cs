using Backend.Fx.Persistence.Abstractions;
using JetBrains.Annotations;
using Npgsql;

namespace Backend.Fx.Persistence.Postgres;

[PublicAPI]
public class PostgresTcpSocketAvailabilityAwaiter : TcpSocketAvailabilityAwaiter
{
    public PostgresTcpSocketAvailabilityAwaiter(string connectionString)
        : base(
            new NpgsqlConnectionStringBuilder(connectionString).Host!,
            new NpgsqlConnectionStringBuilder(connectionString).Port)
    { }
}