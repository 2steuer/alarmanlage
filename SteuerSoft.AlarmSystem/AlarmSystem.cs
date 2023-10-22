using Appccelerate.StateMachine;
using Appccelerate.StateMachine.AsyncMachine;
using NLog;
using SteuerSoft.AlarmSystem.Core.Enums;
using SteuerSoft.AlarmSystem.Core.Interfaces;
using SteuerSoft.AlarmSystem.Core.Sequences;
using SteuerSoft.AlarmSystem.Core.Tools;
using SteuerSoft.AlarmSystem.Extensions;
using SteuerSoft.AlarmSystem.StatemachineExtensions;

namespace SteuerSoft.AlarmSystem;

public class AlarmSystem : IAlarmSystemConfigurator
{
    private ILogger _log;

    private IAsyncStateMachine<State, Triggers> _stateMachine;

    private List<IPowerToggle> _powerToggles = new();

    private List<IPowerStateSource> _powerStateSources = new();

    private List<Sequence> _preAlarmSequences = new();

    private List<Sequence> _alarmSequences = new();

    private List<Sequence> _powerOnSequences = new();

    private List<Sequence> _powerOffSequences = new();

    private List<IAlarmTrigger> _triggers = new();

    private AlarmSystemReportExtension _reporter;

    private string _name;

    public AlarmSystem(string name, TimeSpan armingDelay, TimeSpan alarmDelay, TimeSpan alarmDelayOnPowerOn)
    {
        _log = LogManager.GetLogger(name, typeof(AlarmSystem));

        _name = name;
        var b = new StateMachineDefinitionBuilder<State, Triggers>();

        b.WithInitialState(State.Off);

        b.In(State.Off)
            .ExecuteOnEntry(() => StartSequences(_powerOffSequences))
            .ExecuteOnExit(() => StopSequences(_powerOffSequences))
            
            .On(Triggers.TogglePower)
            .Goto(State.Arming)
            
            .On(Triggers.PowerOn)
            .Goto(State.Arming);

        b.In(State.Arming)
            .ExecuteOnEntry(() => _stateMachine!.FireDelayed(Triggers.ArmingDelayElapsed, armingDelay))
            .On(Triggers.TogglePower)
            .Goto(State.Off)
            .On(Triggers.PowerOff)
            .Goto(State.Off)
            .On(Triggers.ArmingDelayElapsed)
                .If(() => _triggers.Any(t => t.InAlarmState()))
                    .Goto(State.PreAlarm)
                    .Execute(() => _stateMachine!.FireDelayed(Triggers.PreAlarmDelayElapsed, alarmDelayOnPowerOn))
                .Otherwise()
                    .Goto(State.Idle);

        b.In(State.Idle)
            .ExecuteOnEntry(() => StartSequences(_powerOnSequences))
            .ExecuteOnExit(() => StopSequences(_powerOnSequences))
            .On(Triggers.TogglePower)
            .Goto(State.Off)
            .On(Triggers.PowerOff)
            .Goto(State.Off)
            .On(Triggers.Alarm)
                .Goto(State.PreAlarm)
                .Execute(() => _stateMachine!.FireDelayed(Triggers.PreAlarmDelayElapsed, alarmDelay))
            .On(Triggers.ImmediateAlarm)
            .Goto(State.Alarm);

        b.In(State.PreAlarm)
            .ExecuteOnEntry(() => StartSequences(_preAlarmSequences))
            .ExecuteOnExit(() => StopSequences(_preAlarmSequences))
            .On(Triggers.TogglePower)
            .Goto(State.Off)
            .On(Triggers.PowerOff)
            .Goto(State.Off)
            .On(Triggers.PreAlarmDelayElapsed)
            .Goto(State.Alarm);

        b.In(State.Alarm)
            .ExecuteOnEntry(() => StartSequences(_alarmSequences))
            .ExecuteOnExit(() => StopSequences(_alarmSequences))
            .On(Triggers.TogglePower)
            .Goto(State.Off)
            .On(Triggers.PowerOff)
            .Goto(State.Off);

        var d = b.Build();

        _stateMachine = d.CreateActiveStateMachine(name);

        _reporter = new AlarmSystemReportExtension(name);
        _stateMachine.AddExtension(_reporter);
        _stateMachine.AddExtension(new StateMachineLoggingExtension<State,Triggers>($"AlarmSys:{name}"));
    }

    public Task Start() => _stateMachine.Start();

    public Task Stop() => _stateMachine.Stop();

    public Task TogglePower() => _stateMachine.Fire(Triggers.TogglePower);

    public Task SetPower(bool on) => _stateMachine.Fire(on ? Triggers.PowerOn : Triggers.PowerOff);

    public Task Alarm() => _stateMachine.Fire(Triggers.Alarm);

    public Task ImmediateAlarm() => _stateMachine.Fire(Triggers.ImmediateAlarm);

    private void PowerToggleReceived(object? sender, EventArgs e)
    {
        TogglePower();
    }

    private void PowerStateReceived(object? sender, PowerStateEventArgs e)
    {
        SetPower(e.PowerState);
    }

    private async void TriggerReceived(object? sender, TriggerEventArgs e)
    {
        if (sender is IAlarmTrigger trig)
        {
            await _reporter.ReportTrigger(_name, e.TriggerName, e.Type);
        }

        switch (e.Type)
        {
            case TriggerType.Alarm:
                await Alarm();
                break;

            case TriggerType.ImmediateAlarm:
                await ImmediateAlarm();
                break;
        }
    }


    private Task StartSequences(List<Sequence> sequences)
    {
        return Task.WhenAll(sequences.Select(s => s.Start()));
    }

    private Task StopSequences(List<Sequence> sequences)
    {
        return Task.WhenAll(sequences.Select(s => s.Stop()));
    }

    public IAlarmSystemConfigurator WithPowerOffSequence(Sequence seq)
    {
        _powerOffSequences.Add(seq);
        return this;
    }

    public IAlarmSystemConfigurator WithPowerOnSequence(Sequence seq)
    {
        _powerOnSequences.Add(seq);
        return this;
    }

    public IAlarmSystemConfigurator WithPreAlarmSequence(Sequence seq)
    {
        _preAlarmSequences.Add(seq);
        return this;
    }

    public IAlarmSystemConfigurator WithAlarmSequence(Sequence seq)
    {
        _alarmSequences.Add(seq);
        return this;
    }

    public IAlarmSystemConfigurator WithPowerToggle(IPowerToggle toggle)
    {
        _powerToggles.Add(toggle);
        toggle.TogglePower += PowerToggleReceived;
        return this;
    }

    public IAlarmSystemConfigurator WithPowerSwitch(IPowerStateSource powerStateSource)
    {
        _powerStateSources.Add(powerStateSource);
        powerStateSource.PowerStateChanged += PowerStateReceived;
        return this;
    }

    public IAlarmSystemConfigurator WithTrigger(IAlarmTrigger trigger)
    {
        _triggers.Add(trigger);
        trigger.Triggered += TriggerReceived;
        return this;
    }

    public IAlarmSystemConfigurator WithReporter(IAlarmSystemReporter reporter)
    {
        _reporter.Reporters.Add(reporter);
        return this;
    }
}