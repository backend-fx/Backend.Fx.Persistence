using System;
using System.Data.Common;
using JetBrains.Annotations;

namespace Backend.Fx.Persistence.Postgres.Sequences;

[PublicAPI]
public abstract class PostgresIntSequence : PostgresSequence<int>
{
    protected PostgresIntSequence(DbDataSource dbDataSource, int startWith = 1)
        : base(dbDataSource, startWith)
    {
    }

    protected override int ConvertNextValueFromSequence(object valueFromSequence)
    {
        return Convert.ToInt32(valueFromSequence);
    }
}
