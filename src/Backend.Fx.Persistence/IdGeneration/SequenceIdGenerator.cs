using Backend.Fx.Persistence.Sequences;
using JetBrains.Annotations;

namespace Backend.Fx.Persistence.IdGeneration;

[PublicAPI]
public class SequenceIdGenerator<TId>
{
    private readonly ISequence<TId> _sequence;

    public SequenceIdGenerator(ISequence<TId> sequence)
    {
        _sequence = sequence;
    }

    public TId NextId()
    {
        return _sequence.GetNextValue();
    }
}
