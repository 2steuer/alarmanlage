using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.AlarmSystem.Core.Interfaces;
using SteuerSoft.AlarmSystem.Mqtt.Connector;
using SteuerSoft.AlarmSystem.Mqtt.Triggers;

namespace SteuerSoft.AlarmSystem.Mqtt
{
    public static class ConnectorExtensions
    {
        public static IDigitalOutput CreateDigitalOutput(this MqttConnector conn, string name, string publishTopic, string onPayload = "ON", string offPayload = "OFF", bool invert = false, bool defaultState = false)
        {
            return new MqttDigitalOutput(conn, name, publishTopic, onPayload, offPayload, invert, defaultState);
        }

        public static IDigitalInput CreateDigitalInput(this MqttConnector conn, string name, string topic, string onPayload="ON", string offPayload="OFF", bool invert = false)
        {
            var digin = new MqttDigitalInput(name, topic, offPayload, onPayload, invert);
            conn.AddSubscriber(digin).Wait();
            return digin;
        }
    }
}
