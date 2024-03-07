// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using MQTTnet.Diagnostics;
using NLog;
using SteuerSoft.AlarmSystem;
using SteuerSoft.AlarmSystem.Core.Enums;
using SteuerSoft.AlarmSystem.Core.Sequences;
using SteuerSoft.AlarmSystem.Core.Sequences.Actions;
using SteuerSoft.AlarmSystem.Mqtt;
using SteuerSoft.AlarmSystem.Mqtt.Connector;
using SteuerSoft.AlarmSystem.Mqtt.Reporter;
using SteuerSoft.AlarmSystem.TelegramBot;

var log = LogManager.GetCurrentClassLogger();

AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
{
    log.Fatal(eventArgs.ExceptionObject as Exception, $"Unhandled exception in program!");
};

var cfgb = new ConfigurationBuilder();
    


if (args.Length <= 0)
{
    cfgb.AddJsonFile("config.json")
        .AddJsonFile("config.overrides.json", optional: true);
}
else
{
    log.Info($"Loading config {args[0]}");
    cfgb.AddJsonFile(args[0]);

    for (int i = 1; i < args.Length; i++)
    {
        log.Info($"Loading override config {args[i]}");
        cfgb.AddJsonFile(args[i], optional: true);
    }
}

var cfg = cfgb.Build();

log.Info($"Starting Telegram...");

var telegram = new AlarmSystemBot(cfg["Telegram:ApiKey"], cfg["Telegram:Password"], cfg["Telegram:PersistenceStorage"]);
telegram.SendInputStateChangesWhenOff = false;
telegram.SendInputStateChanges = true;

await telegram.Start();

log.Info($"Initializing Alarm System...");

var sys = new AlarmSystem(cfg["AlarmSystemName"], TimeSpan.FromSeconds(cfg.GetValue<int>("PreArmDelay")),
    TimeSpan.FromSeconds(cfg.GetValue<int>("AlarmDelay")), 
    TimeSpan.FromSeconds(cfg.GetValue<int>("AlarmDelayOnPowerOn")));
sys.WithTelegram(telegram);

log.Info($"Initializing MQTT handling...");
var mqtt = new MqttConnector(cfg.GetValue<string>("Mqtt:Server"), cfg.GetValue<int>("Mqtt:Port"), "AlarmAnlage!");

var beepHinten = mqtt.CreateDigitalOutput("Buzzer hinten", "io/output/relay1/set");
var sirene = mqtt.CreateDigitalOutput("Sirene außen", "io/output/relay2/set");
var beepVorn = mqtt.CreateDigitalOutput("Buzzer vorn", "io/output/relay3/set");
var horn = mqtt.CreateDigitalOutput("Horn", "io/output/relay4/set");
var r5 = mqtt.CreateDigitalOutput("Relay 5", "io/output/relay5/set");
var r6 = mqtt.CreateDigitalOutput("Relay 6", "io/output/relay6/set");
var r7 = mqtt.CreateDigitalOutput("Relay 7", "io/output/relay7/set");
var r8 = mqtt.CreateDigitalOutput("Relay 8", "io/output/relay8/set");

var sw1 = mqtt.CreateDigitalInput("Hauptschalter", "io/input/in1");

var doorFront = mqtt.CreateDigitalInput("Tür Hauptraum", "io/input/in2");
var doorTechnic = mqtt.CreateDigitalInput("Tür Technikraum", "io/input/in3");
var doorTools = mqtt.CreateDigitalInput("Tür Geräteraum", "io/input/in4");


sys.WithPowerSwitch("Hauptschalter", sw1);

sys.WithTrigger(doorFront, TriggerType.Alarm);
sys.WithTrigger(doorTechnic, TriggerType.ImmediateAlarm);
sys.WithTrigger(doorTools, TriggerType.ImmediateAlarm);

var digStatePublisher = new DigitalInputStateReporter(mqtt, "alarmanlage-huette/inputs/state");
digStatePublisher.AddDigitalInput(sw1, "An", "Aus");
digStatePublisher.AddDigitalInput(doorFront, "Offen", "Geschlossen");
digStatePublisher.AddDigitalInput(doorTechnic, "Offen", "Geschlossen");
digStatePublisher.AddDigitalInput(doorTools, "Offen", "Geschlossen");


var alarmSequence1 = new Sequence("Alarm", true);
alarmSequence1.Switch(beepHinten, true)
    .Switch(beepVorn, true)
    .Switch(sirene, true)
    .Delay(TimeSpan.FromSeconds(60));

var hornSequence = new Sequence("Horn", false);
hornSequence.Delay(TimeSpan.FromSeconds(20))
    .Repeat(3, builder =>
    {
        builder.SwitchOnFor(horn, TimeSpan.FromSeconds(6))
            .Delay(TimeSpan.FromSeconds(4))
            .SwitchOnFor(horn, TimeSpan.FromSeconds(6))
            .Delay(TimeSpan.FromSeconds(4))
            .SwitchOnFor(horn, TimeSpan.FromSeconds(6))
            .Delay(TimeSpan.FromSeconds(60));
    });


sys.WithAlarmSequence(alarmSequence1);
sys.WithAlarmSequence(hornSequence);

var powerToggleSequence = new Sequence("Power Toggle", false);
powerToggleSequence.SwitchOnFor(beepHinten, TimeSpan.FromSeconds(0.5));
powerToggleSequence.Delay(TimeSpan.FromSeconds(0.5));
powerToggleSequence.SwitchOnFor(beepHinten, TimeSpan.FromSeconds(0.5));

sys.WithPowerOnSequence(powerToggleSequence);
sys.WithPowerOffSequence(powerToggleSequence);

var preAlarmSequence = new Sequence("Pre-Alarm", true);
preAlarmSequence.SwitchOnFor(beepVorn, TimeSpan.FromSeconds(1))
    .Delay(TimeSpan.FromSeconds(1));

sys.WithPreAlarmSequence(preAlarmSequence);

log.Info("Starting up the system...");

sys.WithMqttStateReporter(mqtt, "alarmanlage-huette/state");

await mqtt.Start();
await sys.Start();

log.Info("System running!");

TaskCompletionSource cancelSource = new TaskCompletionSource();

bool haveSigInt = false;

Console.CancelKeyPress += (sender, eventArgs) =>
{
    log.Debug("Processing SIGINT");
    eventArgs.Cancel = true;
    haveSigInt = true;
    cancelSource.TrySetResult();
};

AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
{
    if (!haveSigInt)
    {
        log.Debug("Processing SIGTERM");
        cancelSource.TrySetResult();
    }
    else
    {
        log.Debug($"Got SIGTERM but ignoring it because of SIGINT before");
    }
};

await cancelSource.Task;

log.Info("System shutting down");

await sys.Stop();
await telegram.Stop();
await mqtt.Stop();

log.Info("System was completely shut down. Bye!");

return 0;