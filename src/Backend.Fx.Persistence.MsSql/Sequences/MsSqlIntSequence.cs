using System;
using Backend.Fx.Persistence.AdoNet;
using JetBrains.Annotations;

namespace Backend.Fx.Persistence.MsSql.Sequences;

[PublicAPI]
public abstract class MsSqlIntSequence : MsSqlSequence<int>
{
    protected MsSqlIntSequence(IDbConnectionFactory dbConnectionFactory, int startWith = 1)
        : base(dbConnectionFactory, startWith)
    {
    }

    protected override int ConvertNextValueFromSequence(object valueFromSequence)
    {
        return Convert.ToInt32(valueFromSequence);
    }
}