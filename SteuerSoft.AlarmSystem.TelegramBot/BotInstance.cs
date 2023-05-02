using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Appccelerate.StateMachine;
using Appccelerate.StateMachine.AsyncMachine;
using SteuerSoft.AlarmSystem.Core.Enums;
using SteuerSoft.AlarmSystem.Core.Tools;
using SteuerSoft.AlarmSystem.TelegramBot.Helpers;
using SteuerSoft.AlarmSystem.TelegramBot.Persistence;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using static SteuerSoft.AlarmSystem.TelegramBot.Helpers.KeyboardHelpers;

namespace SteuerSoft.AlarmSystem.TelegramBot;

internal class BotInstance
{
    public long ChatId { get; }

    private TelegramBotClient _bot;

    private IAsyncStateMachine<BotInstanceState, BotInstanceTrigger> _stateMachine;

    private bool _muted = false;
    private DateTime _unmuteTime = DateTime.MinValue;

    private Message? _currentMenuMessage = null;

    private Action<bool> _powerSetter;
    private Action<TriggerType, string> _trigger;

    private TelegramBotPersistence _persistence;

    private string _userName = string.Empty;

    public BotInstance(long chatId, TelegramBotClient bot, Func<string, bool> authorizer, Action<bool> setPower, Action<TriggerType, string> trigger, string persistencePath)
    {
        ChatId = chatId;
        _bot = bot;
        _powerSetter = setPower;
        _trigger = trigger;

        _persistence = new TelegramBotPersistence(persistencePath, chatId);

            var sdb =
            new StateMachineDefinitionBuilder<BotInstanceState, BotInstanceTrigger>();

        sdb.WithInitialState(BotInstanceState.Unauthorized);

        sdb.DefineHierarchyOn(BotInstanceState.Unauthorized)
            .WithHistoryType(HistoryType.None)
            .WithInitialSubState(BotInstanceState.UnauthorizedIdle)
            .WithSubState(BotInstanceState.UnauthorizedRequestLogin);

        sdb.DefineHierarchyOn(BotInstanceState.Authorized)
            .WithHistoryType(HistoryType.None)
            .WithInitialSubState(BotInstanceState.AuthorizedMain)
            .WithSubState(BotInstanceState.AuthorizedAlarm)
            .WithSubState(BotInstanceState.AuthorizedPower);

        sdb.In(BotInstanceState.Authorized)
            .On(BotInstanceTrigger.Logout)
            .Goto(BotInstanceState.Unauthorized);

        sdb.In(BotInstanceState.UnauthorizedIdle)
            .ExecuteOnEntry(SendUnauthorizedIdle)
            .On(BotInstanceTrigger.RequestLogin)
            .Goto(BotInstanceState.UnauthorizedRequestLogin)
            .Execute(SendUnauthorizedLoginRequest)
                
            .On(BotInstanceTrigger.Message)
            .Goto(BotInstanceState.UnauthorizedIdle);

        sdb.In(BotInstanceState.UnauthorizedRequestLogin)
            .On(BotInstanceTrigger.Message)
            .If<string>(s => authorizer(s))
            .Goto(BotInstanceState.AuthorizedMain)
                
            .Otherwise()
            .Goto(BotInstanceState.UnauthorizedIdle)
            .Execute(SendLoginFailed);

        sdb.In(BotInstanceState.AuthorizedMain)
            .ExecuteOnEntry(SendMainMenu)
            .On(BotInstanceTrigger.GotoPageAlarm)
            .Goto(BotInstanceState.AuthorizedAlarm)
                
            .On(BotInstanceTrigger.GotoPagePower)
            .Goto(BotInstanceState.AuthorizedPower)
                
            .On(BotInstanceTrigger.Mute)
            .If(() => !_muted)
            .Goto(BotInstanceState.AuthorizedMain)
            .Execute(Mute)
                
                
            .On(BotInstanceTrigger.UnMute)
            .If(() => _muted)
            .Goto(BotInstanceState.AuthorizedMain)
            .Execute(Unmute);

        sdb.In(BotInstanceState.AuthorizedAlarm)
            .ExecuteOnEntry(SendAlarmMenu)
            .On(BotInstanceTrigger.Alarm)
            .Goto(BotInstanceState.AuthorizedMain)
            .Execute(Alarm)

            .On(BotInstanceTrigger.ImmediateAlarm)
            .Goto(BotInstanceState.AuthorizedMain)
            .Execute(ImmediateAlarm)
                
            .On(BotInstanceTrigger.GoBack)
            .Goto(BotInstanceState.AuthorizedMain);

        sdb.In(BotInstanceState.AuthorizedPower)
            .ExecuteOnEntry(SendPowerMenu)
            .On(BotInstanceTrigger.PowerOn)
            .Goto(BotInstanceState.AuthorizedMain)
            .Execute(PowerOn)

            .On(BotInstanceTrigger.PowerOff)
            .Goto(BotInstanceState.AuthorizedMain)
            .Execute(PowerOff)

            .On(BotInstanceTrigger.GoBack)
            .Goto(BotInstanceState.AuthorizedMain);


        var sd = sdb.Build();

        _stateMachine = sd.CreatePassiveStateMachine();
        _stateMachine.AddExtension(new StateMachineLoggingExtension<BotInstanceState,BotInstanceTrigger>($"TelegramBot:{ChatId}"));
        
    }

    public async Task Start()
    {
        await _stateMachine.Load(_persistence);
        await _stateMachine.Start();

        var chat = await _bot.GetChatAsync(ChatId);

        var userName = chat.Username;
        var firstName = chat.FirstName;
        var lastName = chat.LastName;

        var nameParts = new string[] { firstName, lastName, userName }.Where(s => !string.IsNullOrEmpty(s));

        _userName = string.Join(" ", nameParts);
    }

    public async Task Stop()
    {
        await _stateMachine.Stop();
        await _stateMachine.Save(_persistence);
    }

    private async Task ClearKeyboard()
    {
        if (_currentMenuMessage != null && (_currentMenuMessage.ReplyMarkup?.InlineKeyboard.Any() ?? false))
        {

            await _bot.EditMessageReplyMarkupAsync(ChatId, _currentMenuMessage.MessageId,
                InlineKeyboardMarkup.Empty());
        }
    }

    private async Task SendMenu(string message, InlineKeyboardMarkup? inlineKeyboard = null)
    {
        _currentMenuMessage = await _bot.SendTextMessageAsync(chatId: ChatId,
            text: message,
            parseMode: ParseMode.Markdown,
            replyMarkup: inlineKeyboard);
    }

    public Task SendMessage(string message)
    {
        return _bot.SendTextMessageAsync(chatId: ChatId, text: message, parseMode: ParseMode.Markdown);
    }

    private async Task SendUnauthorizedIdle()
    {
        await ClearKeyboard();

        InlineKeyboardMarkup m =
            new InlineKeyboardMarkup(
                Button("Einloggen", BotInstanceTrigger.RequestLogin));

        await SendMenu("Sie sind nicht angemeldet.", m);
    }

    private async Task SendUnauthorizedLoginRequest()
    {
        await ClearKeyboard();
        await SendMenu("Passwort:");
    }

    private Task SendLoginFailed()
    {
        return SendMessage("Login fehlgeschlagen!");
    }

    private async Task SendMainMenu()
    {
        var m = Keyboard(
            Row(Button("Alarmanlage an / aus", BotInstanceTrigger.GotoPagePower)),
            Row(Button("Test-Alarme", BotInstanceTrigger.GotoPageAlarm)),
            Row(Button(_muted ? "Stumm aus" : "Für 3h Stummschalten",
                _muted ? BotInstanceTrigger.UnMute : BotInstanceTrigger.Mute)),
            Row(Button("Abmelden", BotInstanceTrigger.Logout))
        );

        await ClearKeyboard();
        await SendMenu("Hauptmenü", m);
    }

    private async Task SendPowerMenu()
    {
        var m = Keyboard(
            Row(
                Button("AUS", BotInstanceTrigger.PowerOff),
                Button("AN", BotInstanceTrigger.PowerOn)
            ),
            Row(Button("Zurück", BotInstanceTrigger.GoBack))
        );

        await ClearKeyboard();
        await SendMenu("Zustand wählen", m);
    }

    private async Task SendAlarmMenu()
    {
        var m = Keyboard(
            Row(Button("Normaler Alarm", BotInstanceTrigger.Alarm)),
            Row(Button("Sofortalarm!", BotInstanceTrigger.ImmediateAlarm)),
            Row(Button("Zurück", BotInstanceTrigger.GoBack)));

        await ClearKeyboard();
        await SendMenu("Funktion wählen:", m);
    }

    private async Task Mute()
    {
        _muted = true;
        _unmuteTime = DateTime.Now + TimeSpan.FromHours(3);

        await SendMessage("Benachrichtigungen für drei Stunden stumm geschaltet.");
    }

    private async Task Unmute()
    {
        _muted = false;

        await SendMessage("Stummschaltung aufgehoben.");
    }

    private async Task PowerOn()
    {
        _powerSetter?.Invoke(true);

        await SendMessage("Schalte Alarmanlage an...");
    }

    private async Task PowerOff()
    {
        _powerSetter?.Invoke(false);

        await SendMessage("Schalte Alarmanlage aus...");
    }

    private async Task Alarm()
    {
        await SendMessage("Löse normalen Alarm aus...");

        _trigger?.Invoke(TriggerType.Alarm, _userName);
    }

    private async Task ImmediateAlarm()
    {
        await SendMessage("Löse Sofortalarm aus...");

        _trigger?.Invoke(TriggerType.ImmediateAlarm, _userName);
    }

    public async Task ProcessUpdate(Update update)
    {
        switch (update.Type)
        {
            case UpdateType.CallbackQuery:
                await ProcessCallback(update);
                break;

            case UpdateType.Message:
                await ProcessMessage(update);
                break;
        }
    }

    private Task ProcessCallback(Update update)
    {
        var d = update.CallbackQuery.Data;

        if (string.IsNullOrEmpty(d))
        {
            return Task.CompletedTask;
        }

        if (Enum.TryParse<BotInstanceTrigger>(d, true, out BotInstanceTrigger trig))
        {
            return _stateMachine.Fire(trig);
        }
        else
        {
            return _stateMachine.Fire(BotInstanceTrigger.Callback, d);
        }
    }

    private Task ProcessMessage(Update message)
    {
        var txt = message.Message.Text;

        if (string.IsNullOrEmpty(txt))
        {
            return Task.CompletedTask;
        }

        return _stateMachine.Fire(BotInstanceTrigger.Message, txt);
    }
}