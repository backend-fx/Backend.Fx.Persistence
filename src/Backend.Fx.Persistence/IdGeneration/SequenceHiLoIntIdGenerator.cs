using System.Threading;
using Backend.Fx.Logging;
using Backend.Fx.Persistence.Sequences;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Persistence.IdGeneration;

[PublicAPI]
public class SequenceHiLoIntIdGenerator : SequenceHiLoIdGenerator<int>
{
    private readonly ILogger _logger = Log.Create<HiLoIntIdGenerator>();
    private int _highId = -1;
    private int _lowId = -1;
    private readonly bool _isTraceEnabled;
        
    public SequenceHiLoIntIdGenerator(ISequence<int> sequence) : base(sequence)
    {
        _isTraceEnabled = _logger.IsEnabled(LogLevel.Trace);
    }

    protected override void EnsureValidLowAndHiId()
    {
        if (_lowId == -1 || _lowId > _highId)
        {
            // first fetch from sequence in life time
            _lowId = GetNextBlockStart();
            _highId = _lowId + BlockSize- 1;
        }
    }

    protected override int GetNextId()
    {
        var nextId = _lowId;
        Interlocked.Increment(ref _lowId);
        if (_isTraceEnabled) _logger.LogTrace("Providing id {NextId}", nextId);
        return nextId;
    }
}