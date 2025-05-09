﻿using System;
using System.Data;
using System.Data.Common;
using Backend.Fx.Logging;
using Backend.Fx.Persistence.Sequences;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Persistence.MsSql.Sequences;

[PublicAPI]
public abstract class MsSqlSequence<TId> : ISequence<TId>
{
    private readonly ILogger _logger = Log.Create<MsSqlSequence<TId>>();
    private readonly DbDataSource _dbDataSource;
    private readonly int _startWith;

    protected MsSqlSequence(DbDataSource dbDataSource, int startWith = 1)
    {
        _dbDataSource = dbDataSource;
        _startWith = startWith;
    }

    protected abstract string SequenceName { get; }

    protected virtual string SchemaName { get; } = "dbo";

    public void EnsureSequence()
    {
        _logger.LogInformation("Ensuring existence of mssql sequence {SchemaName}.{SequenceName}", SchemaName,
            SequenceName);
        using IDbConnection dbConnection = _dbDataSource.CreateConnection();
        dbConnection.Open();
        bool sequenceExists;
        using (IDbCommand cmd = dbConnection.CreateCommand())
        {
            cmd.CommandText = $"SELECT count(*) FROM sys.sequences seq " +
                              $"join sys.schemas s on s.schema_id  = seq.schema_id " +
                              $"WHERE seq.name = '{SequenceName}' and s.name = '{SchemaName}'";
            sequenceExists = (int)cmd.ExecuteScalar()! == 1;
        }

        if (sequenceExists)
        {
            _logger.LogInformation("Sequence {SchemaName}.{SequenceName} exists", SchemaName, SequenceName);
        }
        else
        {
            _logger.LogInformation("Sequence {SchemaName}.{SequenceName} does not exist yet and will be created now",
                SchemaName, SequenceName);
            using IDbCommand cmd = dbConnection.CreateCommand();
            cmd.CommandText =
                $"CREATE SEQUENCE [{SchemaName}].[{SequenceName}] START WITH {_startWith} INCREMENT BY {Increment}";
            cmd.ExecuteNonQuery();
            _logger.LogInformation("Sequence {SchemaName}.{SequenceName} created", SchemaName, SequenceName);
        }
    }

    public TId GetNextValue()
    {
        using IDbConnection dbConnection = _dbDataSource.CreateConnection();
        dbConnection.Open();
        using IDbCommand selectNextValCommand = dbConnection.CreateCommand();
        selectNextValCommand.CommandText = $"SELECT next value FOR {SchemaName}.{SequenceName}";
        TId nextValue = ConvertNextValueFromSequence(
            selectNextValCommand.ExecuteScalar()
            ?? throw new InvalidOperationException("Getting next value from sequence returned NULL"));
        _logger.LogDebug("{SchemaName}.{SequenceName} served {NextValue} as next value", SchemaName, SequenceName,
            nextValue);

        return nextValue;
    }

    public abstract TId Increment { get; }

    protected abstract TId ConvertNextValueFromSequence(object valueFromSequence);
}
