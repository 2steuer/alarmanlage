using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.AlarmSystem.Core.Enums;
using SteuerSoft.AlarmSystem.Core.Interfaces;
using SteuerSoft.AlarmSystem.Mqtt.Connector;

namespace SteuerSoft.AlarmSystem.Mqtt.Triggers
{
    internal class MqttDigitalInputTrigger : IAlarmTrigger, ISubscriber
    {
        public event EventHandler<TriggerEventArgs>? Triggered;
        public string Filter { get; }

        private TriggerType _triggerType;
        private string _name;
        private string _offPayload;
        private string _onPayload;

        private bool _invert; // If 'true' the trigger is sent on the _offPayload value, i.e. input is inverted

        public MqttDigitalInputTrigger(string name, TriggerType type, string mqttfilter, string offPayload, string onPayload, bool invert)
        {
            _name = name;
            _triggerType = type;
            Filter = mqttfilter;
            _offPayload = offPayload;
            _onPayload = onPayload;
            _invert = invert;
        }

        public Task HandleMessage(string topic, string payload)
        {
            bool active = payload == _onPayload;
            bool inActive = payload == _offPayload;

            if (!active && !inActive)
            {
                // unknown payload
                return Task.CompletedTask;
            }

            if (active || (inActive && _invert))
            {
                Triggered?.Invoke(this, new TriggerEventArgs(_triggerType, _name));
            }

            return Task.CompletedTask;
        }
    }
}
