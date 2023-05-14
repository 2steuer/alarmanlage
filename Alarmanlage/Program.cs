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

await mqtt.Start();

var alarmSequence = new Sequence("Alarm", true);
alarmSequence.Delay(TimeSpan.FromSeconds(0.5));
alarmSequence.SwitchOnFor(r1, TimeSpan.FromSeconds(1));

sys.WithAlarmSequence(alarmSequence);

await sys.Start();

Console.ReadLine();

await sys.Stop();
await telegram.Stop();
await mqtt.Stop();