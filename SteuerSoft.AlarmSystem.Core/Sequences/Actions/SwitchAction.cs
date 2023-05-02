using SteuerSoft.AlarmSystem.Core.Interfaces;

namespace SteuerSoft.AlarmSystem.Core.Sequences.Actions;

public class SwitchAction : ISequenceEntry
{
    private IDigitalOutput _switch;
    private bool _desiredState;

    public SwitchAction(IDigitalOutput @switch, bool desiredState)
    {
        _switch = @switch;
        _desiredState = desiredState;
    }

    public Task Execute(CancellationToken ctx)
    {
        return _switch.Set(_desiredState, ctx);
    }
}