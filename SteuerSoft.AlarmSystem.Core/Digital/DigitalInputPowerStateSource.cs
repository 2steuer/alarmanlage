using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.AlarmSystem.Core.Interfaces;

namespace SteuerSoft.AlarmSystem.Core.Digital
{
    public class DigitalInputPowerStateSource : IPowerStateSource, IDisposable
    {
        public event EventHandler<PowerStateEventArgs>? PowerStateChanged;
        public string Name { get; }

        private IDigitalInput _in;
        private bool _onState;

        public DigitalInputPowerStateSource(string name, IDigitalInput @in, bool onState)
        {
            Name = name;
            _in = @in;
            _onState = onState;

            _in.OnStateChanged += _in_OnStateChanged;
        }

        private void _in_OnStateChanged(object? sender, DigitalInputStateEventArgs e)
        {
            bool on = e.NewState == _onState;

            PowerStateChanged?.Invoke(this, new PowerStateEventArgs(on));
        }

        public void Dispose()
        {
            _in.OnStateChanged -= _in_OnStateChanged;
        }
    }
}
