using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace SteuerSoft.AlarmSystem.TelegramBot.Helpers
{
    internal static class KeyboardHelpers
    {
        public static InlineKeyboardMarkup Keyboard(params IEnumerable<InlineKeyboardButton>[] rows)
        {
            return new InlineKeyboardMarkup(rows);
        }

        public static IEnumerable<InlineKeyboardButton> Row(params InlineKeyboardButton[] btns)
        {
            return btns;
        }

        public static InlineKeyboardButton Button(string txt, BotInstanceTrigger callback)
        {
            return InlineKeyboardButton.WithCallbackData(txt, callback.ToString());
        }
    }
}
