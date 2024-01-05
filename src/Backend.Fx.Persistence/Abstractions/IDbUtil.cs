using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NodaTime;

namespace Backend.Fx.Persistence.Abstractions;

[PublicAPI]
public interface IDbUtil
{
    string ConnectionString { get; }
    bool WaitUntilAvailable();
    bool WaitUntilAvailable(Duration timeout);
    Task<bool> WaitUntilAvailableAsync(CancellationToken cancellationToken);
    Task<bool> WaitUntilAvailableAsync(Duration timeout, CancellationToken cancellationToken);
    void EnsureDroppedDatabase(string dbName);
    bool ExistsDatabase(string dbName);
    bool ExistsTable(string schemaName, string tableName);
    void CreateDatabase(string dbName);
    void CreateSchema(string schemaName, string? createScriptContent = null);
}