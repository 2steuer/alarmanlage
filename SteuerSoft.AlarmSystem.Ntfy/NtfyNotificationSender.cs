using System.Text;
using ntfy;
using ntfy.Requests;
using SteuerSoft.AlarmSystem.Core.Enums;
using SteuerSoft.AlarmSystem.Core.Interfaces;

namespace SteuerSoft.AlarmSystem.Ntfy;

public class NtfyNotificationSender : IAlarmSystemReporter
{
    private readonly Client _client;
    private readonly User _user;

    private string _topic;

    public NtfyNotificationSender(string url, string topic, string apiToken)
    {
        _client = new Client(url);
        _user = new User(apiToken);
        _topic = topic;
    }


    public Task NewState(string name, State newState)
    {
        var msg = new SendingMessage();
        
        switch (newState)
        {
            case State.Off:
            msg.Title = "Alarmanlage";
            msg.Message = "Alarmanlage ausgeschaltet.";
            msg.Priority = PriorityLevel.Default;
            break;

            case State.Arming:
            msg.Title = "Alarmanlage";
            msg.Message = "Alarmanlage wird bereit.";
            msg.Priority = PriorityLevel.Default;
            break;

            case State.Idle:
            msg.Title = "Alarmanlage";
            msg.Message = "Alarmanlage eingeschaltet und bereit.";
            msg.Priority = PriorityLevel.Default;
            break;

            case State.PreAlarm:
            msg.Title = "VORALARM AUSGELÖST";
            msg.Tags = new [] {"rotating_light"};
            msg.Message = "Alarmanlage wurde ausgelöst! Voralarm aktiviert.";
            msg.Priority = PriorityLevel.High;
            break;

            case State.Alarm:
            msg.Title = "ALARM ALARM ALARM";
            msg.Tags = new [] {"rotating_light", "oncoming_police_car"};
            msg.Message = "ALARMANLAGE AUSGELÖST";
            msg.Priority = PriorityLevel.Max;
            break;

            case State.TestAlarm:
            msg.Title = "Alarmanlage";
            msg.Message = "Alarmanlage ausgeschaltet.";
            msg.Priority = PriorityLevel.High;
            break;

            default:
            msg.Title = "Alarmanlage: Neuer Zustand";
            msg.Message = $"Alarmanlage: {newState}.";
            msg.Priority = PriorityLevel.Default;
            break;
        }

        return _client.Publish(_topic, msg, _user);
    }

    public Task NewTrigger(string name, string triggerName, TriggerType type)
    {
        var msg = new SendingMessage();

        msg.Title = "Auslöser detektiert!";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"*Auslöser:* {triggerName}");
        sb.AppendLine($"*Typ:* {type switch
        {
            TriggerType.Test => "Test",
            TriggerType.Alarm => "Normal",
            TriggerType.ImmediateAlarm => "Sofort",
            _ => "Unbekannt"
        }}");

        msg.Message = sb.ToString();
        msg.Priority = PriorityLevel.Default;
        msg.Tags = new [] {"warning"};

        return _client.Publish(_topic, msg, _user);
    }
}
