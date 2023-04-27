using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.AlarmSystem.TelegramBot;

internal enum BotInstanceState
{
    Unauthorized,
    UnauthorizedIdle,
    UnauthorizedRequestLogin,

    Authorized,
    AuthorizedMain,
    AuthorizedPower,
    AuthorizedAlarm

}