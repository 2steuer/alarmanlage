using SteuerSoft.AlarmSystem.Core.Sequences.Actions.Base;

namespace SteuerSoft.AlarmSystem.Core.Sequences.Actions;

public class SwitchAction : ISequenceEntry
{
    private ISwitch _switch;
    private bool _desiredState;

    public SwitchAction(ISwitch @switch, bool desiredState)
    {
        _switch = @switch;
        _desiredState = desiredState;
    }

    public Task Execute(CancellationToken ctx)
    {
        return _switch.Switch(_desiredState, ctx);
    }
}