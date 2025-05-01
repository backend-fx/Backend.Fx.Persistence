using System;
using System.Data.Common;
using Backend.Fx.Persistence.AdoNet;
using JetBrains.Annotations;

namespace Backend.Fx.Persistence.Postgres.Sequences;

[PublicAPI]
public abstract class PostgresLongSequence : PostgresSequence<long>
{
    protected PostgresLongSequence(DbDataSource dbDataSource, int startWith = 1)
        : base(dbDataSource, startWith)
    {
    }

    protected override long ConvertNextValueFromSequence(object valueFromSequence)
    {
        return Convert.ToInt64(valueFromSequence);
    }
}
