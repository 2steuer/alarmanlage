using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.AlarmSystem.TelegramBot.Persistence
{
    internal class PersistenceObject
    {
        public BotInstanceState? CurrentState { get; set; }

        public Dictionary<BotInstanceState, BotInstanceState> HistoryStates { get; set; } = new();
    }
}
