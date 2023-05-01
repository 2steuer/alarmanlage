using Appccelerate.StateMachine;
using Appccelerate.StateMachine.AsyncMachine;
using Appccelerate.StateMachine.AsyncMachine.States;
using Appccelerate.StateMachine.AsyncMachine.Transitions;
using Appccelerate.StateMachine.Infrastructure;
using NLog;

namespace SteuerSoft.AlarmSystem.Core.Tools
{
    public class StateMachineLoggingExtension<TState, TTrigger> : IExtension<TState, TTrigger>
        where TState:IComparable
        where TTrigger:IComparable
    {
        private ILogger _log;

        public StateMachineLoggingExtension(string name)
        {
            _log = LogManager.GetLogger(name, this.GetType());
        }

        public Task StartedStateMachine(IStateMachineInformation<TState, TTrigger> stateMachine)
        {
            _log.Info("State machine started.");
            return Task.CompletedTask;
        }

        public Task StoppedStateMachine(IStateMachineInformation<TState, TTrigger> stateMachine)
        {
            _log.Info("State machine stopped.");
            return Task.CompletedTask;
        }

        public Task EventQueued(IStateMachineInformation<TState, TTrigger> stateMachine, TTrigger eventId, object eventArgument)
        {
            _log.Trace($"Event queued: {eventId}, current State: {stateMachine.CurrentStateId.GetString()}");
            return Task.CompletedTask;
        }

        public Task EventQueuedWithPriority(IStateMachineInformation<TState, TTrigger> stateMachine, TTrigger eventId, object eventArgument)
        {
            _log.Trace($"Priority event queued: {eventId}, current State: {stateMachine.CurrentStateId.GetString()}");
            return Task.CompletedTask;
        }

        public Task SwitchedState(IStateMachineInformation<TState, TTrigger> stateMachine, IStateDefinition<TState, TTrigger> oldStateDefinition,
            IStateDefinition<TState, TTrigger> newStateDefinition)
        {
            _log.Debug($"Switched state: {(oldStateDefinition?.Id.ToString() ?? "NULL")} -> {newStateDefinition.Id}");
            return Task.CompletedTask;
        }

        public Task EnteringInitialState(IStateMachineInformation<TState, TTrigger> stateMachine, TState state)
        {
            _log.Debug($"Entering initial state: {state}");
            return Task.CompletedTask;
        }

        public Task EnteredInitialState(IStateMachineInformation<TState, TTrigger> stateMachine, TState state, ITransitionContext<TState, TTrigger> context)
        {
            _log.Trace($"Entered initial state: {state}");
            return Task.CompletedTask;
        }

        public Task FiringEvent(IStateMachineInformation<TState, TTrigger> stateMachine, ref TTrigger eventId, ref object eventArgument)
        {
            _log.Trace($"Firing event: {eventId}, current state: {stateMachine.CurrentStateId.GetString()}");
            return Task.CompletedTask;
        }

        public Task FiredEvent(IStateMachineInformation<TState, TTrigger> stateMachine, ITransitionContext<TState, TTrigger> context)
        {
            _log.Trace($"Fired event: {context.EventId.GetString()}, current state: {stateMachine.CurrentStateId.GetString()}");
            return Task.CompletedTask;
        }

        public Task HandlingEntryActionException(IStateMachineInformation<TState, TTrigger> stateMachine, IStateDefinition<TState, TTrigger> state,
            ITransitionContext<TState, TTrigger> context, ref Exception exception)
        {
            _log.Warn(exception, $"Error in entry action of {context.StateDefinition.Id}");
            return Task.CompletedTask;
        }

        public Task HandledEntryActionException(IStateMachineInformation<TState, TTrigger> stateMachine, IStateDefinition<TState, TTrigger> state,
            ITransitionContext<TState, TTrigger> context, Exception exception)
        {
            return Task.CompletedTask;
        }

        public Task HandlingExitActionException(IStateMachineInformation<TState, TTrigger> stateMachine, IStateDefinition<TState, TTrigger> state,
            ITransitionContext<TState, TTrigger> context, ref Exception exception)
        {
            _log.Warn(exception, $"Error in exit action of {context.StateDefinition.Id}");
            return Task.CompletedTask;
        }

        public Task HandledExitActionException(IStateMachineInformation<TState, TTrigger> stateMachine, IStateDefinition<TState, TTrigger> state,
            ITransitionContext<TState, TTrigger> context, Exception exception)
        {
            return Task.CompletedTask;
        }

        public Task HandlingGuardException(IStateMachineInformation<TState, TTrigger> stateMachine, ITransitionDefinition<TState, TTrigger> transitionDefinition,
            ITransitionContext<TState, TTrigger> transitionContext, ref Exception exception)
        {
            _log.Warn(exception, $"Error in guard {transitionDefinition.Source.Id} -> {transitionDefinition.Target.Id} with {transitionContext.EventId.GetString()}");
            return Task.CompletedTask;
        }

        public Task HandledGuardException(IStateMachineInformation<TState, TTrigger> stateMachine, ITransitionDefinition<TState, TTrigger> transitionDefinition,
            ITransitionContext<TState, TTrigger> transitionContext, Exception exception)
        {
            return Task.CompletedTask;
        }

        public Task HandlingTransitionException(IStateMachineInformation<TState, TTrigger> stateMachine, ITransitionDefinition<TState, TTrigger> transitionDefinition,
            ITransitionContext<TState, TTrigger> context, ref Exception exception)
        {
            _log.Warn(exception, $"Error in transition {transitionDefinition.Source.Id} -> {transitionDefinition.Target.Id}");
            return Task.CompletedTask;
        }

        public Task HandledTransitionException(IStateMachineInformation<TState, TTrigger> stateMachine, ITransitionDefinition<TState, TTrigger> transitionDefinition,
            ITransitionContext<TState, TTrigger> transitionContext, Exception exception)
        {
            return Task.CompletedTask;
        }

        public Task SkippedTransition(IStateMachineInformation<TState, TTrigger> stateMachineInformation, ITransitionDefinition<TState, TTrigger> transitionDefinition,
            ITransitionContext<TState, TTrigger> context)
        {
            _log.Debug($"Skipped transition from {transitionDefinition} -> {transitionDefinition.Target.Id}");
            return Task.CompletedTask;
        }

        public Task ExecutingTransition(IStateMachineInformation<TState, TTrigger> stateMachineInformation, ITransitionDefinition<TState, TTrigger> transitionDefinition,
            ITransitionContext<TState, TTrigger> context)
        {
            _log.Debug($"Executing transition from {transitionDefinition.Source.Id} -> {transitionDefinition.Target.Id} with {context.EventId.GetString()}");
            return Task.CompletedTask;
        }

        public Task ExecutedTransition(IStateMachineInformation<TState, TTrigger> stateMachineInformation, ITransitionDefinition<TState, TTrigger> transitionDefinition,
            ITransitionContext<TState, TTrigger> context)
        {
            _log.Debug($"Executed transition from {transitionDefinition.Source.Id} -> {transitionDefinition.Target.Id} with {context.EventId.GetString()}");
            return Task.CompletedTask;
        }

        public Task EnteringState(IStateMachineInformation<TState, TTrigger> stateMachineInformation, IStateDefinition<TState, TTrigger> state,
            ITransitionContext<TState, TTrigger> context)
        {
            _log.Debug($"Entering {state.Id}");
            return Task.CompletedTask;
        }

        public Task Loaded(IStateMachineInformation<TState, TTrigger> stateMachineInformation, IInitializable<TState> loadedCurrentState,
            IReadOnlyDictionary<TState, TState> loadedHistoryStates, IReadOnlyCollection<EventInformation<TTrigger>> events, IReadOnlyCollection<EventInformation<TTrigger>> priorityEvents)
        {
            _log.Debug($"Successfully loaded state. Last state: {loadedCurrentState.GetString()}");
            return Task.CompletedTask;
        }
    }
}
