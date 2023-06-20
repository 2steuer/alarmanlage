using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.AlarmSystem.Core.Enums;
using SteuerSoft.AlarmSystem.Core.Interfaces;
using SteuerSoft.AlarmSystem.Mqtt.Connector;

namespace SteuerSoft.AlarmSystem.Mqtt.Reporter
{
    internal class MqttStateReporter : IAlarmSystemReporter
    {
        private IPublisher _publisher;

        private string _stateTopic;

        public MqttStateReporter(IPublisher publisher, string topic)
        {
            _publisher = publisher;
            _stateTopic = topic;
        }

        public Task NewState(string name, State newState)
        {
            return _publisher.PublishMessage(_stateTopic, newState.ToString(), true);
        }

        public Task NewTrigger(string name, string triggerName, TriggerType type)
        {
            return Task.CompletedTask;
        }
    }
}
