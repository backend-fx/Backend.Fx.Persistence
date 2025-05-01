using System;
using System.Data.Common;
using JetBrains.Annotations;

namespace Backend.Fx.Persistence.Oracle.Sequences;

[PublicAPI]
public abstract class OracleIntSequence : OracleSequence<int>
{
    protected OracleIntSequence(DbDataSource dbDataSource, int startWith = 1)
        : base(dbDataSource, startWith)
    {
    }

    protected override int ConvertNextValueFromSequence(object valueFromSequence)
    {
        return Convert.ToInt32(valueFromSequence);
    }
}
