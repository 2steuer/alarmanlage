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

namespace SteuerSoft.AlarmSystem.TelegramBot;

public class AlarmSystemBot : IAlarmSystemReporter, IPowerStateSource, IAlarmTrigger
{
    private CancellationTokenSource _cancelTokenSource = new();

    private TelegramBotClient _bot;
    private string _password;
    public bool Running { get; private set; } = false;

    private Dictionary<long, BotInstance> _instances = new();

    private string _persistencePath;

    public AlarmSystemBot(string apiToken, string password, string persistencePath)
    {
        _bot = new TelegramBotClient(apiToken);
        _password = password;
        _persistencePath = persistencePath;
    }

    private bool CheckPassword(string pw)
    {
        return pw.Equals(_password);
    }

    public async Task Start()
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

        if (!Directory.Exists(_persistencePath))
        {
            Directory.CreateDirectory(_persistencePath);
        }

        foreach (string file in Directory.GetFiles(_persistencePath, "*.json"))
        {
            var num = Path.GetFileNameWithoutExtension(file);

            if (long.TryParse(num, out long chatId))
            {
                var newInstance = new BotInstance(chatId, _bot, CheckPassword, SetPower, Trigger, _persistencePath);
                
                await newInstance.Start();

                _instances.Add(chatId, newInstance);
            }
        }

        _bot.StartReceiving(new DefaultUpdateHandler(HandleUpdate, PollingErrorHandler), cancellationToken:_cancelTokenSource.Token);
    }

    public async Task Stop()
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

        foreach (var botInstance in _instances)
        {
            await botInstance.Value.Stop();
        }

        _instances.Clear();
    }

    private Task PollingErrorHandler(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
    {
        // TODO: LOGGING
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
            var newInstane = new BotInstance(chatId, _bot, CheckPassword, SetPower, Trigger, _persistencePath);
            await newInstane.Start();

            _instances.Add(chatId, newInstane);
        }

        await _instances[chatId].ProcessUpdate(update);
    }

    private void SetPower(bool powerState)
    {
        try
        {
            PowerStateChanged?.Invoke(this, new PowerStateEventArgs(powerState));
        }
        catch (Exception e)
        {
            // TODO: log
        }
    }

    private void Trigger(TriggerType triggerType, string triggerName)
    {
        try
        {
            StringBuilder nameBuilder = new StringBuilder();
            nameBuilder.Append("Telegram");

            if (!string.IsNullOrEmpty(triggerName))
            {
                nameBuilder.Append(": ");
                nameBuilder.Append(triggerName);
            }

            Triggered?.Invoke(this, new TriggerEventArgs(triggerType, nameBuilder.ToString()));
        }
        catch (Exception e)
        {
            // TODO: LOG
        }
    }

    public Task NewState(string name, State newState)
    {
        StringBuilder msg = new StringBuilder();
            

        switch (newState)
        {
            case State.Off:
                msg.AppendLine("Alarmanlage ausgeschaltet.");
                break;

            case State.Idle:
                msg.AppendLine("Alarmanlage angeschaltet und bereit.");
                break;

            case State.Arming:
                msg.AppendLine("Alarmanlage wird bereit...");
                break;

            case State.PreAlarm:
                msg.AppendLine("*VORALARM AUSGELÖST!*");
                break;

            case State.Alarm:
                msg.AppendLine("# ALARM!");
                msg.AppendLine("*Die Alarmanlage ist ausgelöst worden. Bitte prüfen und ggf. Schritte einleiten!*");
                break;
        }

        return Task.WhenAll(_instances.Select(kvp => kvp.Value.SendMessage(msg.ToString())));
    }

    public Task NewTrigger(string name, string triggerName, TriggerType type)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"### Auslöser detektiert!");
        sb.AppendLine($"*Auslöser:* {triggerName}");
        sb.AppendLine($"*Typ:* {(type == TriggerType.Alarm ? "Normal" : "Sofort")}");

        return Task.WhenAll(_instances.Select(kvp => kvp.Value.SendMessage(sb.ToString())));
    }

    public event EventHandler<PowerStateEventArgs>? PowerStateChanged;
    public string Name { get; } = "Telegram Bot";

    public event EventHandler<TriggerEventArgs>? Triggered;
}