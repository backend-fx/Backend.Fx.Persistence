using System;
using Backend.Fx.Persistence.AdoNet;
using JetBrains.Annotations;

namespace Backend.Fx.Persistence.Oracle.Sequences;

[PublicAPI]
public abstract class OracleIntSequence : OracleSequence<int>
{
    protected OracleIntSequence(IDbConnectionFactory dbConnectionFactory, int startWith = 1) 
        : base(dbConnectionFactory, startWith)
    {
    }

    protected override int ConvertNextValueFromSequence(object valueFromSequence)
    {
        return Convert.ToInt32(valueFromSequence);
    }
}