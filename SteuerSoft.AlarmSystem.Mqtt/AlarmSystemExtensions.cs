using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.AlarmSystem.Core.Enums;
using SteuerSoft.AlarmSystem.Core.Interfaces;
using SteuerSoft.AlarmSystem.Mqtt.Connector;
using SteuerSoft.AlarmSystem.Mqtt.Triggers;

namespace SteuerSoft.AlarmSystem.Mqtt
{
    public static class AlarmSystemExtensions
    {
        public static IAlarmSystemConfigurator WithMqttTrigger(this IAlarmSystemConfigurator configurator, MqttConnector connector, string name, TriggerType type, string topic, string offTopic = "OFF", string onTopic = "ON", bool invert = false)
        {
            var sub = new MqttDigitalInputTrigger(name, type, topic, offTopic, onTopic, invert);

            connector.AddSubscriber(sub).Wait();
            configurator.WithTrigger(sub);

            return configurator;
        }
    }
}
