using System;
using JetBrains.Annotations;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace Backend.Fx.Persistence.Oracle.Packages;

[PublicAPI]
public abstract class CustomCollectionType<TType, TValue> : CustomType<TType>, IOracleArrayTypeFactory
    where TType : CustomType<TType>, new()
{
    [OracleArrayMapping]
    [PublicAPI]
    public TValue[] Values = Array.Empty<TValue>();

    public Array CreateArray(int elementCount)
    {
        return new TValue[elementCount];
    }

    public Array CreateStatusArray(int elementCount)
    {
        return new OracleUdtStatus[elementCount];
    }

    public override void FromCustomObject(OracleConnection connection, object pointerUdt)
    {
        OracleUdt.SetValue(connection, pointerUdt, 0, Values);
    }

    public override void ToCustomObject(OracleConnection connection, object pointerUdt)
    {
        Values = (TValue[])OracleUdt.GetValue(connection, pointerUdt, 0);
    }
}
