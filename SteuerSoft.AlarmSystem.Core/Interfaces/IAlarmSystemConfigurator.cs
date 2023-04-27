using SteuerSoft.AlarmSystem.Core.Sequences;

namespace SteuerSoft.AlarmSystem.Core.Interfaces;

public interface IAlarmSystemConfigurator
{
    IAlarmSystemConfigurator WithPowerOffSequence(Sequence seq);

    IAlarmSystemConfigurator WithPowerOnSequence(Sequence seq);

    IAlarmSystemConfigurator WithPreAlarmSequence(Sequence seq);

    IAlarmSystemConfigurator WithAlarmSequence(Sequence seq);

    IAlarmSystemConfigurator WithPowerToggle(IPowerToggle toggle);

    IAlarmSystemConfigurator WithPowerSwitch(IPowerStateSource powerStateSource);

    IAlarmSystemConfigurator WithTrigger(IAlarmTrigger trigger);

    IAlarmSystemConfigurator WithReporter(IAlarmSystemReporter reporter);
}