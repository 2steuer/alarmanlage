using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.AlarmSystem;
using SteuerSoft.AlarmSystem.Core.Enums;
using SteuerSoft.AlarmSystem.Core.Interfaces;
using SteuerSoft.AlarmSystem.Mqtt.Connector;
using SteuerSoft.AlarmSystem.Mqtt.Triggers;

namespace SteuerSoft.AlarmSystem.Mqtt
{
    public static class AlarmSystemExtensions
    {
        public static IAlarmSystemConfigurator WithMqttTrigger(this IAlarmSystemConfigurator configurator, MqttConnector connector, string name, TriggerType type, string topic, string offTopic = "OFF", string onTopic = "ON", bool triggerState = true)
        {
            var sub = new MqttDigitalInput(name, topic, offTopic, onTopic, false);

            connector.AddSubscriber(sub).Wait();

            configurator.WithTrigger(sub, type, triggerState);

            return configurator;
        }
    }
}
