using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.AlarmSystem.Sequences;

namespace SteuerSoft.AlarmSystem.Interfaces
{
    public interface IAlarmSystemConfigurator
    {
        IAlarmSystemConfigurator WithPowerOffSequence(Sequence seq);

        IAlarmSystemConfigurator WithPowerOnSequence(Sequence seq);

        IAlarmSystemConfigurator WithPreAlarmSequence(Sequence seq);

        IAlarmSystemConfigurator WithAlarmSequence(Sequence seq);

        IAlarmSystemConfigurator WithPowerToggle(IPowerToggle toggle);

        IAlarmSystemConfigurator WithPowerSwitch(IPowerStateSource powerStateSource);
    }
}
