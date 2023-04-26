namespace SteuerSoft.AlarmSystem.Core.Interfaces
{
    public class TriggerEventArgs : EventArgs
    {
        public TriggerType Type { get; }

        public TriggerEventArgs(TriggerType type)
        {
            Type = type;
        }
    }

    public interface IAlarmTrigger
    {
        string Name { get; }

        event EventHandler<TriggerEventArgs> Triggered;
    }
}
