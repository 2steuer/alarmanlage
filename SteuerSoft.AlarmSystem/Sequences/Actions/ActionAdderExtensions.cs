using SteuerSoft.AlarmSystem.Sequences.Actions.Base;

namespace SteuerSoft.AlarmSystem.Sequences.Actions;

public static class ActionAdderExtensions
{
    public static Sequence Delay(this Sequence seq, TimeSpan delay)
    {
        return seq.Add(new DelayAction(delay));
    }

    public static Sequence Switch(this Sequence seq, ISwitch actuator, bool desiredState)
    {
        return seq.Add(new SwitchAction(actuator, desiredState));
    }

    public static Sequence SwitchOnFor(this Sequence seq, ISwitch actuator, TimeSpan onTime)
    {
        return seq.Switch(actuator, true)
            .Delay(onTime)
            .Switch(actuator, false);
    }
}