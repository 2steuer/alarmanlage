using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Appccelerate.StateMachine;
using Appccelerate.StateMachine.AsyncMachine;
using Appccelerate.StateMachine.AsyncMachine.States;
using NLog;
using SteuerSoft.AlarmSystem.Core.Enums;
using SteuerSoft.AlarmSystem.Core.Interfaces;

namespace SteuerSoft.AlarmSystem.StatemachineExtensions;

internal class AlarmSystemReportExtension : AsyncExtensionBase<State, Triggers>
{
    private static ILogger _log = LogManager.GetCurrentClassLogger();

    public List<IAlarmSystemReporter> Reporters { get; } = new();

    private string _machineName;

    public AlarmSystemReportExtension(string name)
    {
        _machineName = name;
    }

    public override async Task SwitchedState(IStateMachineInformation<State, Triggers> stateMachine, IStateDefinition<State, Triggers> oldState, IStateDefinition<State, Triggers> newState)
    {
        try
        {
            await Task.WhenAll(Reporters.Select(r => r.NewState(_machineName, newState.Id)));
        }
        catch (Exception e)
        {
            _log.Error(e, $"Error while SwitchedState handler.");
        }
    }

    public async Task ReportTrigger(string machineName, string triggerName, TriggerType type)
    {
        try
        {
            await Task.WhenAll(Reporters.Select(r => r.NewTrigger(machineName, triggerName, type)));
        }
        catch (Exception e)
        {
            _log.Error(e, "Error while ReportTrigger.");
        }
    }
}