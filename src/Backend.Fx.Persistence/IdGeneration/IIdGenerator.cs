using JetBrains.Annotations;

namespace Backend.Fx.Persistence.IdGeneration;

[PublicAPI]
public interface IIdGenerator<out TId>
{
    TId NextId();
}
