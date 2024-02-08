using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using Backend.Fx.Exceptions;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Persistence.Oracle.Packages;

[PublicAPI]
public abstract class OraclePackage(OracleConnection oracleConnection, string packageName)
{
    private const int ErrorInExecutingOdciIndexStartRoutineErrorCode = 29902;
    private const int OracleTextErrorCode = 20000;
    private const string WildcardQueryExpansionResultedInTooManyTerms = "DRG-51030";
    private const string TextQueryParserSyntaxError = "DRG-50901";

    private readonly ILogger _logger = Log.Create<OraclePackage>();

    protected string PackageName { get; } = packageName;

    protected TResult[] CallFunction<TResult>(
        string functionName,
        Func<IDataReader, TResult> read,
        params OracleParameter[] parameters)
    {
        using (_logger.LogDebugDuration($"Calling {PackageName}.{functionName}"))
        {
            string paramDeclarations = string.Join(",", parameters.Select(p => $":{p.ParameterName}"));
            var sql = $"SELECT * FROM TABLE({PackageName}.{functionName}({paramDeclarations}))";

            var command = oracleConnection.CreateCommand();
            command.BindByName = true;
            command.CommandText = sql;
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddRange(parameters);
            LogSqlCommand(command);

            using (var dataReader = command.ExecuteReader())
            {
                var result = new Collection<TResult>();
                while (dataReader != null && dataReader.Read() && dataReader.HasRows)
                {
                    result.Add(read(dataReader));
                }

                return result.ToArray();
            }
        }
    }

    protected void CallProcedure(string procedureName, params OracleParameter[] parameters)
    {
        using (_logger.LogDebugDuration($"Calling {PackageName}.{procedureName}"))
        {
            try
            {
                var command = new OracleCommand($"{PackageName}.{procedureName}")
                {
                    BindByName = true,
                    CommandType = CommandType.StoredProcedure,
                    Connection = oracleConnection
                };

                command.Parameters.AddRange(parameters);
                LogSqlCommand(command);
                command.ExecuteNonQuery();
            }
            catch (OracleException oracleException)
            {
                throw HandleSpecificOracleExceptions(oracleException);
            }
        }
    }

    protected TResult[] CallProcedure<TResult>(
        string procedureName,
        Func<OracleDataReader, TResult> read,
        params OracleParameter[] parameters)
    {
        using (_logger.LogDebugDuration($"Calling {PackageName}.{procedureName}"))
        {
            try
            {
                var result = new Collection<TResult>();

                var command = new OracleCommand($"{PackageName}.{procedureName}")
                {
                    BindByName = true,
                    CommandType = CommandType.StoredProcedure,
                    Connection = oracleConnection
                };

                command.Parameters.AddRange(parameters);
                LogSqlCommand(command);
                using (var dataReader = command.ExecuteReader())
                {
                    while (dataReader != null && dataReader.Read() && dataReader.HasRows)
                    {
                        result.Add(read(dataReader));
                    }
                }

                return result.ToArray();
            }
            catch (OracleException oracleException)
            {
                throw HandleSpecificOracleExceptions(oracleException);
            }
        }
    }

    private void LogSqlCommand(OracleCommand command)
    {
        var sb = new StringBuilder();
        sb.Append($"Executing {command.CommandType} command [{command.CommandText} (");
        for (var index = 0; index < command.Parameters.Count; index++)
        {
            if (index > 0) sb.Append(", ");
            sb.Append($"{command.Parameters[index].ParameterName}={ValueToString(command.Parameters[index])}");
        }

        sb.Append(")]");

        _logger.LogDebug(sb.ToString());
    }

    private static string ValueToString(OracleParameter param)
    {
        if (param.Value == null)
        {
            return "NULL";
        }

        return param.Value.ToString() ?? "NULL";
    }

    private Exception HandleSpecificOracleExceptions(OracleException oracleException)
    {
        _logger.LogDebug(oracleException, "Oracle Exception occured with code: {ErrorNumber}", oracleException.Number);
        if (IsTextIndexQueryParserSyntaxError(oracleException))
        {
            return new ClientException(
                    "Stored procedure call failed with: query parser syntax error",
                    oracleException)
                .AddError(i18n.QueryParserSyntaxErrorMessage);
        }

        if (IsTextIndexWildcardQueryExpansionResultedInTooManyTerms(oracleException))
        {
            return new ClientException(
                    "Stored procedure call failed with: query expansion resulted in too may terms",
                    oracleException)
                .AddError(i18n.QueryExpansionTooManyTermsErrorMessage);
        }

        return oracleException;
    }

    private static bool IsTextIndexWildcardQueryExpansionResultedInTooManyTerms(OracleException oracleException)
    {
        return (oracleException.Number == ErrorInExecutingOdciIndexStartRoutineErrorCode ||
                oracleException.Number == OracleTextErrorCode) &&
            oracleException.Message.Contains(WildcardQueryExpansionResultedInTooManyTerms);
    }

    private static bool IsTextIndexQueryParserSyntaxError(OracleException oracleException)
    {
        return oracleException.Number == ErrorInExecutingOdciIndexStartRoutineErrorCode &&
            oracleException.Message.Contains(TextQueryParserSyntaxError);
    }
}
