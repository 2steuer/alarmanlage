using SteuerSoft.AlarmSystem.Core.Enums;

namespace SteuerSoft.AlarmSystem.Core.Interfaces;

public class TriggerEventArgs : EventArgs
{
    public TriggerType Type { get; }

    public string TriggerName { get; }

    public TriggerEventArgs(TriggerType type, string triggerName)
    {
        Type = type;
        TriggerName = triggerName;
    }
}

public interface IAlarmTrigger
{
    event EventHandler<TriggerEventArgs> Triggered;
}