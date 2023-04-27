using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.AlarmSystem.Core.Interfaces;

namespace SteuerSoft.AlarmSystem.TelegramBot;

public static class AlarmSystemConfigExtensions
{
    public static IAlarmSystemConfigurator WithTelegram(this IAlarmSystemConfigurator cfg, AlarmSystemBot bot)
    {
        return cfg.WithPowerSwitch(bot)
            .WithTrigger(bot)
            .WithReporter(bot);
    }
}