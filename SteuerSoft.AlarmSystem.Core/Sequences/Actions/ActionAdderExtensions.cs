using SteuerSoft.AlarmSystem.Core.Interfaces;

namespace SteuerSoft.AlarmSystem.Core.Sequences.Actions;

public static class ActionAdderExtensions
{
    public static ISequenceBuilder Delay(this ISequenceBuilder seq, TimeSpan delay)
    {
        return seq.AddAction(new DelayAction(delay));
    }

    public static ISequenceBuilder Switch(this ISequenceBuilder seq, IDigitalOutput actuator, bool desiredState)
    {
        return seq.AddAction(new SwitchAction(actuator, desiredState));
    }

    public static ISequenceBuilder SwitchOnFor(this ISequenceBuilder seq, IDigitalOutput actuator, TimeSpan onTime)
    {
        return seq.Switch(actuator, true)
            .Delay(onTime)
            .Switch(actuator, false);
    }

    public static ISequenceBuilder Repeat(this ISequenceBuilder seq, int repeatCount, Action<ISequenceBuilder>? actions)
    {
        var ra = new RepeatAction(repeatCount);
        actions?.Invoke(ra);

        return seq.AddAction(ra);
    }
}