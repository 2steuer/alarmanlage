using SteuerSoft.AlarmSystem.Core.Interfaces;

namespace SteuerSoft.AlarmSystem.Core.Sequences.Actions;

public static class ActionAdderExtensions
{
    public static Sequence Delay(this Sequence seq, TimeSpan delay)
    {
        return seq.Add(new DelayAction(delay));
    }

    public static Sequence Switch(this Sequence seq, IDigitalOutput actuator, bool desiredState)
    {
        return seq.Add(new SwitchAction(actuator, desiredState));
    }

    public static Sequence SwitchOnFor(this Sequence seq, IDigitalOutput actuator, TimeSpan onTime)
    {
        return seq.Switch(actuator, true)
            .Delay(onTime)
            .Switch(actuator, false);
    }
}