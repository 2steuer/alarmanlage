using Appccelerate.StateMachine;

namespace SteuerSoft.AlarmSystem.Extensions;

internal static class StateMachineExtensions
{
    public static void FireDelayed<TState, TTrigger>(this IAsyncStateMachine<TState, TTrigger> stateMachine, TTrigger trigger, TimeSpan delay)
        where TState:IComparable
        where TTrigger:IComparable
    {
        ThreadPool.QueueUserWorkItem(a =>
        {
            Thread.Sleep(delay);
            stateMachine.Fire(trigger);
        });
    }
}