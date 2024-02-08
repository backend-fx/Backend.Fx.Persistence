using JetBrains.Annotations;

namespace Backend.Fx.Persistence.Sequences;

[PublicAPI]
public interface ISequence<out TId> 
{
    void EnsureSequence();
    TId GetNextValue();
    TId Increment { get; }
}