using System;
using System.Data;
using System.Data.Common;
using Backend.Fx.Logging;
using Backend.Fx.Persistence.Sequences;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Persistence.Oracle.Sequences;

public abstract class OracleSequence<TId> : ISequence<TId>
{
    private readonly ILogger _logger = Log.Create<OracleSequence<TId>>();
    private readonly DbDataSource _dbConnectionFactory;
    private readonly int _startWith;

    protected OracleSequence(DbDataSource dbDataSource, int startWith = 1)
    {
        _dbConnectionFactory = dbDataSource;
        _startWith = startWith;
    }

    protected abstract string SequenceName { get; }

    protected abstract string SchemaName { get; }

    private string SchemaPrefix
    {
        get
        {
            if (string.IsNullOrEmpty(SchemaName)) return string.Empty;

            return SchemaName + ".";
        }
    }

    public void EnsureSequence()
    {
        _logger.LogInformation(
            "Ensuring existence of oracle sequence {SchemaPrefix}.{SequenceName}", SchemaPrefix, SequenceName);

        using IDbConnection dbConnection = _dbConnectionFactory.CreateConnection();
        dbConnection.Open();
        bool sequenceExists;
        using (IDbCommand command = dbConnection.CreateCommand())
        {
            command.CommandText = $"SELECT count(*) FROM user_sequences WHERE sequence_name = '{SequenceName}'";
            sequenceExists = (decimal)command.ExecuteScalar()! == 1m;
        }

        if (sequenceExists)
        {
            _logger.LogInformation("Oracle sequence {SchemaPrefix}.{SequenceName} exists", SchemaPrefix, SequenceName);
        }
        else
        {
            _logger.LogInformation(
                "Oracle sequence {SchemaPrefix}.{SequenceName} does not exist yet and will be created now",
                SchemaPrefix,
                SequenceName);
            using IDbCommand cmd = dbConnection.CreateCommand();
            cmd.CommandText
                = $"CREATE SEQUENCE {SchemaPrefix}{SequenceName} START WITH {_startWith} INCREMENT BY {Increment}";
            cmd.ExecuteNonQuery();
            _logger.LogInformation("Oracle sequence {SchemaPrefix}.{SequenceName} created", SchemaPrefix, SequenceName);
        }
    }

    public TId GetNextValue()
    {
        using IDbConnection dbConnection = _dbConnectionFactory.CreateConnection();
        dbConnection.Open();

        using IDbCommand command = dbConnection.CreateCommand();
        command.CommandText = $"SELECT {SchemaPrefix}{SequenceName}.NEXTVAL FROM dual";
        TId nextValue = ConvertNextValueFromSequence(
            command.ExecuteScalar()
            ?? throw new InvalidOperationException("Getting next value from sequence returned NULL"));
        _logger.LogDebug(
            "Oracle sequence {SchemaPrefix}.{SequenceName} served {NextValue} as next value",
            SchemaPrefix,
            SequenceName,
            nextValue);

        return nextValue;
    }

    public abstract TId Increment { get; }

    protected abstract TId ConvertNextValueFromSequence(object valueFromSequence);
}
