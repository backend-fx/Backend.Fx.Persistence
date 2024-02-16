using Backend.Fx.Persistence.Sequences;

namespace Backend.Fx.Persistence.IdGeneration;

public abstract class SequenceHiLoIdGenerator<TId> : HiLoIdGenerator<TId>
{
    private readonly ISequence<TId> _sequence;

    protected SequenceHiLoIdGenerator(ISequence<TId> sequence)
    {
        _sequence = sequence;
    }

    protected override TId GetNextBlockStart()
    {
        return _sequence.GetNextValue();
    }

    protected override TId BlockSize => _sequence.Increment;
}