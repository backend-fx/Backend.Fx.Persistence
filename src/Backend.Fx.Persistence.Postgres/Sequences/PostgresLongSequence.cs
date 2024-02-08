using System;
using Backend.Fx.Persistence.AdoNet;
using JetBrains.Annotations;

namespace Backend.Fx.Persistence.Postgres.Sequences;

[PublicAPI]
public abstract class PostgresLongSequence : PostgresSequence<long>
{
    protected PostgresLongSequence(IDbConnectionFactory dbConnectionFactory, int startWith = 1)
        : base(dbConnectionFactory, startWith)
    {
    }

    protected override long ConvertNextValueFromSequence(object valueFromSequence)
    {
        return Convert.ToInt64(valueFromSequence);
    }
}