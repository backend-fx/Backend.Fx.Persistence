using System;
using Backend.Fx.Persistence.AdoNet;
using JetBrains.Annotations;

namespace Backend.Fx.Persistence.Oracle.Sequences;

[PublicAPI]
public abstract class OracleLongSequence : OracleSequence<long>
{
    protected OracleLongSequence(IDbConnectionFactory dbConnectionFactory, int startWith = 1) 
        : base(dbConnectionFactory, startWith)
    {
    }

    protected override long ConvertNextValueFromSequence(object valueFromSequence)
    {
        return Convert.ToInt64(valueFromSequence);
    }
}