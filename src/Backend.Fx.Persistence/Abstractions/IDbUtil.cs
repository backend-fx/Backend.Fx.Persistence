using JetBrains.Annotations;

namespace Backend.Fx.Persistence.Abstractions;

[PublicAPI]
public interface IDbUtil
{
    string ConnectionString { get; }
    void EnsureDroppedDatabase(string dbName);
    bool ExistsDatabase(string dbName);
    bool ExistsTable(string schemaName, string tableName);
    void CreateDatabase(string dbName);
    void CreateSchema(string schemaName, string? createScriptContent = null);
}