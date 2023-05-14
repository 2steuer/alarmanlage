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
    }
}
