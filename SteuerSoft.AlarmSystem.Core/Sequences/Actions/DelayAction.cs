using SteuerSoft.AlarmSystem.Core.Interfaces;

namespace SteuerSoft.AlarmSystem.Core.Sequences.Actions;

public class DelayAction : ISequenceEntry
{
    private readonly TimeSpan _delay;

    public DelayAction(TimeSpan delay)
    {
        _delay = delay;
    }

    public Task Execute(CancellationToken ctx)
    {
        return Task.Delay(_delay, ctx);
    }
}