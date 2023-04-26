using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.AlarmSystem.Core.Enums;
using SteuerSoft.AlarmSystem.Core.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SteuerSoft.AlarmSystem.TelegramBot
{
    public class AlarmSystemBot : IAlarmSystemReporter
    {
        private CancellationTokenSource _cancelTokenSource = new();

        private TelegramBotClient _bot;
        private string _password;
        public bool Running { get; private set; } = false;

        private Dictionary<long, BotInstance> _instances = new();

        public AlarmSystemBot(string apiToken, string password)
        {
            _bot = new TelegramBotClient(apiToken);
            _password = password;
        }

        private bool CheckPassword(string pw)
        {
            return pw.Equals(_password);
        }

        public void Start()
        {
            lock (this)
            {
                if (Running)
                {
                    return;
                }

                Running = true;
                _cancelTokenSource = new CancellationTokenSource();
            }

            _bot.StartReceiving(new DefaultUpdateHandler(HandleUpdate, PollingErrorHandler), cancellationToken:_cancelTokenSource.Token);
        }

        public void Stop()
        {
            lock (this)
            {
                if (!Running)
                {
                    return;
                }

                _cancelTokenSource.Cancel();
                Running = false;
            }

            
        }

        private Task PollingErrorHandler(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            Debugger.Break();
            return Task.CompletedTask;
        }

        private Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken ctx)
        {
            switch (update.Type)
            {
                case UpdateType.CallbackQuery:
                    return ProcessUpdate(update.CallbackQuery.Message.Chat.Id, update);

                case UpdateType.Message:
                    return ProcessUpdate(update.Message.Chat.Id, update);
            }

            return Task.CompletedTask;
        }

        private async Task ProcessUpdate(long chatId, Update update)
        {
            if (!_instances.ContainsKey(chatId))
            {
                var newInstane = new BotInstance(chatId, _bot, CheckPassword);
                await newInstane.Start();

                _instances.Add(chatId, newInstane);
            }

            await _instances[chatId].ProcessUpdate(update);
        }

        public void NewState(string name, State newState)
        {
            throw new NotImplementedException();
        }

        public void NewTrigger(string name, string triggerName, TriggerType type)
        {
            throw new NotImplementedException();
        }
    }
}
