using System;
using System.Data.Common;
using JetBrains.Annotations;

namespace Backend.Fx.Persistence.MsSql.Sequences;

[PublicAPI]
public abstract class MsSqlLongSequence : MsSqlSequence<long>
{
    protected MsSqlLongSequence(DbDataSource dbDataSource, int startWith = 1)
        : base(dbDataSource, startWith)
    {
    }

    protected override long ConvertNextValueFromSequence(object valueFromSequence)
    {
        return Convert.ToInt64(valueFromSequence);
    }
}
