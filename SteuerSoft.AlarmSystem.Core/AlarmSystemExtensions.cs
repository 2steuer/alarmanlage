using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.AlarmSystem.Core.Digital;
using SteuerSoft.AlarmSystem.Core.Enums;
using SteuerSoft.AlarmSystem.Core.Interfaces;

namespace SteuerSoft.AlarmSystem
{
    public static class AlarmSystemExtensions
    {
        public static IAlarmSystemConfigurator WithTrigger(this IAlarmSystemConfigurator configurator, IDigitalInput digin, TriggerType type, bool triggerState = true)
        {
            var trig = new DigitalInputTrigger(digin, type, triggerState);
            configurator.WithTrigger(trig);

            return configurator;
        }

        public static IAlarmSystemConfigurator WithPowerSwitch(this IAlarmSystemConfigurator configurator,
            string name,
            IDigitalInput input,
            bool onState = true)
        {
            var ps = new DigitalInputPowerStateSource(name, input, onState);
            return configurator.WithPowerSwitch(ps);
        }
    }
}
