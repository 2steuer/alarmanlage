using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.AlarmSystem.Core.Enums;
using SteuerSoft.AlarmSystem.Core.Interfaces;

namespace SteuerSoft.AlarmSystem.Core.Digital
{
    public class DigitalInputTrigger : IAlarmTrigger, IDisposable
    {
        public event EventHandler<TriggerEventArgs>? Triggered;

        private IDigitalInput _in;
        private bool _triggerState;
        private TriggerType _Type;

        public DigitalInputTrigger(IDigitalInput @in, TriggerType type, bool triggerState = true)
        {
            _in = @in;
            _triggerState = triggerState;
            _Type = type;

            _in.OnStateChanged += DigitalInputChanged;
        }

        private void DigitalInputChanged(object? sender, DigitalInputStateEventArgs e)
        {
            if (e.NewState == _triggerState)
            {
                Triggered?.Invoke(this, new TriggerEventArgs(_Type, $"Digital In: {e.Name}"));
            }
        }

        public void Dispose()
        {
            _in.OnStateChanged -= DigitalInputChanged;
        }
    }
}
