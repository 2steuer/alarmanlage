using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.AlarmSystem.Mqtt.Connector
{
    public interface IPublisher
    {
        Task PublishMessage(string topic, string payload, bool retain = false, CancellationToken ctx = default);
    }
}
