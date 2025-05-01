using System;
using System.Data;
using System.Data.Common;
using Backend.Fx.Logging;
using Backend.Fx.Persistence.Sequences;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Persistence.Postgres.Sequences;

public abstract class PostgresSequence<TId> : ISequence<TId>
{
    private readonly ILogger _logger = Log.Create<PostgresSequence<TId>>();
    private readonly DbDataSource _dbDataSource;
    private readonly int _startWith;

    protected PostgresSequence(DbDataSource dbDataSource, int startWith = 1)
    {
        _dbDataSource = dbDataSource;
        _startWith = startWith;
    }

    protected abstract string SequenceName { get; }

    protected abstract string SchemaName { get; }

    public void EnsureSequence()
    {
        _logger.LogInformation(
            "Ensuring existence of postgres sequence {SchemaName}.{SequenceName}", SchemaName,
            SequenceName);

        using IDbConnection dbConnection = _dbDataSource.CreateConnection();
        dbConnection.Open();
        bool sequenceExists;
        using (IDbCommand command = dbConnection.CreateCommand())
        {
            command.CommandText =
                $"SELECT count(*) FROM information_schema.sequences " +
                $"WHERE sequence_name = '{SequenceName}' AND sequence_schema = '{SchemaName}'";
            sequenceExists = (long)command.ExecuteScalar()! == 1L;
        }

        if (sequenceExists)
        {
            _logger.LogInformation("Sequence {SchemaName}.{SequenceName} exists", SchemaName, SequenceName);
        }
        else
        {
            _logger.LogInformation(
                "Sequence {SchemaName}.{SequenceName} does not exist yet and will be created now",
                SchemaName,
                SequenceName);
            using IDbCommand cmd = dbConnection.CreateCommand();
            cmd.CommandText =
                $"CREATE SEQUENCE {SchemaName}.{SequenceName} START WITH {_startWith} INCREMENT BY {Increment}";
            cmd.ExecuteNonQuery();
            _logger.LogInformation("Sequence {SchemaName}.{SequenceName} created", SchemaName, SequenceName);
        }
    }

    public TId GetNextValue()
    {
        using IDbConnection dbConnection = _dbDataSource.CreateConnection();
        dbConnection.Open();

        using IDbCommand command = dbConnection.CreateCommand();
        command.CommandText = $"SELECT nextval('{SchemaName}.{SequenceName}');";
        TId nextValue = ConvertNextValueFromSequence(
            command.ExecuteScalar()
            ?? throw new InvalidOperationException("Getting next value from sequence returned NULL"));
        _logger.LogDebug("{SchemaName}.{SequenceName} served {2} as next value", SchemaName, SequenceName, nextValue);

        return nextValue;
    }

    public abstract TId Increment { get; }

    protected abstract TId ConvertNextValueFromSequence(object valueFromSequence);
}
