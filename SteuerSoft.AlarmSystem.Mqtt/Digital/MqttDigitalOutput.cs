using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.AlarmSystem.Core.Interfaces;
using SteuerSoft.AlarmSystem.Mqtt.Connector;

namespace SteuerSoft.AlarmSystem.Mqtt.Triggers
{
    internal class MqttDigitalOutput : IDigitalOutput
    {
        private IPublisher _pub;

        public string Name { get; }

        private string _topic;
        private string _onPayload;
        private string _offPayload;
        private bool _invert;
        private bool _defaultState;

        internal MqttDigitalOutput(IPublisher pub, string name, string topic, string onPayload, string offPayload, bool invert, bool defaultState)
        {
            _pub = pub;
            Name = name;
            _topic = topic;
            _onPayload = onPayload;
            _offPayload = offPayload;
            _invert = invert;
            _defaultState = defaultState;
        }

        public Task Set(bool on, CancellationToken ctx = default)
        {
            var pl = (on || (!on && _invert)) ? _onPayload : _offPayload;

            return _pub.PublishMessage(_topic, pl, ctx:ctx);
        }

        public Task SetDefault(CancellationToken ctx = default)
        {
            return Set(_defaultState, ctx);
        }
    }
}
