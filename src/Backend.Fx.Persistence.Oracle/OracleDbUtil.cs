namespace Backend.Fx.Persistence.Oracle;
//
// public class OracleDbUtil : AdoNetDbUtil
// {
//     public OracleDbUtil(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
//     {
//     }
//
//     protected override string GetIsAvailableCheckCommand() => "select 1 from DUAL";
//
//     protected override string GetCreateSchemaCommand(string schemaName)=> "";
//
//     protected override string GetExistsDatabaseCommand(string dbName)=> "";
//
//     protected override void DropDatabase(string dbName, IDbConnection connection)=> "";
//
//     protected override string GetExistsTableCommand(string schemaName, string tableName)=> "";
//
//     protected override string GetCreateDatabaseCommand(string dbName)=> "";
// }