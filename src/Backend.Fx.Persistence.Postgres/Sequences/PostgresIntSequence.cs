using System;
using Backend.Fx.Persistence.AdoNet;
using JetBrains.Annotations;

namespace Backend.Fx.Persistence.Postgres.Sequences;

[PublicAPI]
public abstract class PostgresIntSequence : PostgresSequence<int>
{
    protected PostgresIntSequence(IDbConnectionFactory dbConnectionFactory, int startWith = 1)
        : base(dbConnectionFactory, startWith)
    {
    }

    protected override int ConvertNextValueFromSequence(object valueFromSequence)
    {
        return Convert.ToInt32(valueFromSequence);
    }
}
