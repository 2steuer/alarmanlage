// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using MQTTnet.Diagnostics;
using SteuerSoft.AlarmSystem;
using SteuerSoft.AlarmSystem.Core.Sequences;
using SteuerSoft.AlarmSystem.Core.Sequences.Actions;
using SteuerSoft.AlarmSystem.Mqtt;
using SteuerSoft.AlarmSystem.Mqtt.Connector;
using SteuerSoft.AlarmSystem.TelegramBot;

var cfgb = new ConfigurationBuilder();
    

if (args.Length <= 0)
{
    cfgb.AddJsonFile("config.json")
        .AddJsonFile("config.overrides.json", optional: true);
}
else
{
    cfgb.AddJsonFile(args[0]);

    for (int i = 1; i < args.Length; i++)
    {
        cfgb.AddJsonFile(args[i], optional: true);
    }
}

var cfg = cfgb.Build();

var telegram = new AlarmSystemBot(cfg["Telegram:ApiKey"], cfg["Telegram:Password"], cfg["Telegram:PersistenceStorage"]);
await telegram.Start();

var sys = new AlarmSystem(cfg["AlarmSystemName"], TimeSpan.FromSeconds(cfg.GetValue<int>("PreArmDelay")),
    TimeSpan.FromSeconds(cfg.GetValue<int>("AlarmDelay")));
sys.WithTelegram(telegram);

var mqtt = new MqttConnector(cfg.GetValue<string>("Mqtt:Server"), cfg.GetValue<int>("Mqtt:Port"), "AlarmAnlage!");

var r1 = mqtt.CreateDigitalOutput("Relay 1", "io/output/relay1/set");
var r2 = mqtt.CreateDigitalOutput("Relay 2", "io/output/relay2/set");
var r3 = mqtt.CreateDigitalOutput("Relay 3", "io/output/relay3/set");
var r4 = mqtt.CreateDigitalOutput("Relay 4", "io/output/relay4/set");
var r5 = mqtt.CreateDigitalOutput("Relay 5", "io/output/relay5/set");
var r6 = mqtt.CreateDigitalOutput("Relay 6", "io/output/relay6/set");
var r7 = mqtt.CreateDigitalOutput("Relay 7", "io/output/relay7/set");
var r8 = mqtt.CreateDigitalOutput("Relay 8", "io/output/relay8/set");

var sw1 = mqtt.CreateDigitalInput("TestSwitch", "test/switch1");

sys.WithPowerSwitch("TestDoor", sw1);


var alarmSequence1 = new Sequence("Alarm 1", true);
alarmSequence1.Delay(TimeSpan.FromSeconds(0.5));
alarmSequence1.SwitchOnFor(r1, TimeSpan.FromSeconds(1));

var alarmSequence2 = new Sequence("Alarm 2", true);
alarmSequence2.Switch(r2, true)
    .Delay(TimeSpan.FromSeconds(0.7))
    .Switch(r3, true)
    .Delay(TimeSpan.FromSeconds(0.7))
    .Switch(r2, false)
    .Delay(TimeSpan.FromSeconds(0.7))
    .Switch(r3, false)
    .Delay(TimeSpan.FromSeconds(0.7));

sys.WithAlarmSequence(alarmSequence1);
sys.WithAlarmSequence(alarmSequence2);

var powerToggleSequence = new Sequence("Power Toggle", false);
powerToggleSequence.SwitchOnFor(r4, TimeSpan.FromSeconds(2));

sys.WithPowerOnSequence(powerToggleSequence);
sys.WithPowerOffSequence(powerToggleSequence);

var preAlarmSequence = new Sequence("Pre-Alarm", true);
preAlarmSequence.SwitchOnFor(r5, TimeSpan.FromSeconds(4))
    .Delay(TimeSpan.FromSeconds(2));

sys.WithPreAlarmSequence(preAlarmSequence);

await mqtt.Start();
await sys.Start();

TaskCompletionSource cancelSource = new TaskCompletionSource();

Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancelSource.TrySetResult();
};

await cancelSource.Task;

await sys.Stop();
await telegram.Stop();
await mqtt.Stop();