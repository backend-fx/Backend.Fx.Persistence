using System;
using System.Data.Common;
using JetBrains.Annotations;

namespace Backend.Fx.Persistence.Oracle.Sequences;

[PublicAPI]
public abstract class OracleLongSequence : OracleSequence<long>
{
    protected OracleLongSequence(DbDataSource dbDataSource, int startWith = 1)
        : base(dbDataSource, startWith)
    {
    }

    protected override long ConvertNextValueFromSequence(object valueFromSequence)
    {
        return Convert.ToInt64(valueFromSequence);
    }
}
