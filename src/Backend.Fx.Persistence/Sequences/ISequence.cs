using JetBrains.Annotations;

namespace Backend.Fx.Persistence.Sequences;

[PublicAPI]
public interface ISequence<out TId>
{
    TId GetNextValue();

    TId Increment { get; }
}
