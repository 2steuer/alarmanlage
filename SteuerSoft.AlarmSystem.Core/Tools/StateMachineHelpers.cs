using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Appccelerate.StateMachine.AsyncMachine;
using Appccelerate.StateMachine.Infrastructure;

namespace SteuerSoft.AlarmSystem.Core.Tools
{
    internal static class StateMachineHelpers
    {
        public static string? GetString<T>(this IInitializable<T> i)
        {
            return i.IsInitialized ? i.ExtractOrThrow()?.ToString() : "N/A";
        }

        public static string? GetString<T>(this Missable<T> m)
        {
            return m.IsMissing ? "N/A" : m.Value?.ToString();
        }
    }
}
