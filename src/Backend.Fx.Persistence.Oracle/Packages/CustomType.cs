using JetBrains.Annotations;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace Backend.Fx.Persistence.Oracle.Packages;

[PublicAPI]
public abstract class CustomType<T> : IOracleCustomType, IOracleCustomTypeFactory, INullable
    where T : CustomType<T>, new()
{
    public static T Null = new() { IsNull = true };

    public bool IsNull { get; private set; }

    /// <summary>
    ///     C# => Oracle
    /// </summary>
    public abstract void FromCustomObject(OracleConnection connection, object pointerUdt);

    /// <summary>
    ///     Oracle => C#
    /// </summary>
    public abstract void ToCustomObject(OracleConnection connection, object pointerUdt);

    public IOracleCustomType CreateObject()
    {
        return new T();
    }
}
