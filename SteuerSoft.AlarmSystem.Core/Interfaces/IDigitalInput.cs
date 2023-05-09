using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.AlarmSystem.Core.Interfaces
{
    public class DigitalInputStateEventArgs : EventArgs
    {
        public string Name { get; }

        public bool NewState { get; }

        public DigitalInputStateEventArgs(string name, bool newState)
        {
            Name = name;
            NewState = newState;
        }
    }

    internal interface IDigitalInput
    {
        event EventHandler<DigitalInputStateEventArgs> OnStateChanged;
    }
}
