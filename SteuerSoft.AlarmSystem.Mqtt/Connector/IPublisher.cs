using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.AlarmSystem.Mqtt.Connector
{
    internal interface IPublisher
    {
        Task Publish(string payload);
    }
}
