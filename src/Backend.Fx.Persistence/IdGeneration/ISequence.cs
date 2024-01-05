using JetBrains.Annotations;

namespace Backend.Fx.Persistence.IdGeneration;

[PublicAPI]
public interface ISequence<out TId> 
{
    void EnsureSequence();
    TId GetNextValue();
    TId Increment { get; }
}