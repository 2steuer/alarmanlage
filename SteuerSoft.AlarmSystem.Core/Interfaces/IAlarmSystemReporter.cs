namespace SteuerSoft.AlarmSystem.Core.Interfaces
{
    public interface IAlarmSystemReporter
    {
        void NewState(string name, State newState);

        void NewTrigger(string name, string triggerName, TriggerType type);
    }
}
