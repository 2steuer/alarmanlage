using SteuerSoft.AlarmSystem.Core.Interfaces;
using SteuerSoft.AlarmSystem.Mqtt.Connector;

namespace SteuerSoft.AlarmSystem.Mqtt.Control;

internal class MqttControlInput : IAlarmTrigger, IPowerStateSource, IPowerToggle, ISubscriber
{
    public string Name => "MQTT";

    public string Filter { get; }

    public event EventHandler? TogglePower;
    public event EventHandler<PowerStateEventArgs>? PowerStateChanged;
    public event EventHandler<TriggerEventArgs>? Triggered;

    public MqttControlInput(string topic)
    {
        Filter = topic;
    }

    public Task HandleMessage(string topic, string payload)
    {
        switch (payload)
        {
            case "TogglePower":
            TogglePower?.Invoke(this, EventArgs.Empty);
            break;

            case "PowerOff":
            PowerStateChanged?.Invoke(this, new PowerStateEventArgs(false));
            break;

            case "PowerOn":
            PowerStateChanged?.Invoke(this, new PowerStateEventArgs(true));
            break;

            case "Alarm":
            Triggered?.Invoke(this, new TriggerEventArgs(Core.Enums.TriggerType.Alarm, Name));
            break;

            case "ImmediateAlarm":
            Triggered?.Invoke(this, new TriggerEventArgs(Core.Enums.TriggerType.ImmediateAlarm, Name));
            break;

            case "TestAlarm":
            Triggered?.Invoke(this, new TriggerEventArgs(Core.Enums.TriggerType.Test, Name));
            break;
        }

        return Task.CompletedTask;
    }

    public bool InAlarmState()
    {
        return false;
    }
}