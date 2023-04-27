using SteuerSoft.AlarmSystem.Core.Enums;

namespace SteuerSoft.AlarmSystem.Core.Interfaces;

public interface IAlarmSystemReporter
{
    Task NewState(string name, State newState);

    Task NewTrigger(string name, string triggerName, TriggerType type);
}