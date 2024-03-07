using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.AlarmSystem.TelegramBot;

internal enum BotInstanceTrigger
{
    Message,
    Callback,
    RequestLogin,
    Mute,
    UnMute,
    PowerOff,
    PowerOn,
    Alarm,
    ImmediateAlarm,
    TestAlarm,
    Logout,

    GotoPageAlarm,
    GotoPagePower,
    GoBack
}