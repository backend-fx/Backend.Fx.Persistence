using System;
using System.Data.Common;
using Backend.Fx.Persistence.AdoNet;
using JetBrains.Annotations;

namespace Backend.Fx.Persistence.MsSql.Sequences;

[PublicAPI]
public abstract class MsSqlIntSequence : MsSqlSequence<int>
{
    protected MsSqlIntSequence(DbDataSource dbDataSource, int startWith = 1)
        : base(dbDataSource, startWith)
    {
    }

    protected override int ConvertNextValueFromSequence(object valueFromSequence)
    {
        return Convert.ToInt32(valueFromSequence);
    }
}
