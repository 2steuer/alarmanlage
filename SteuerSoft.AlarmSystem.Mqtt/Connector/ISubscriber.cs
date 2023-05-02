using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.AlarmSystem.Mqtt.Connector
{
    public interface ISubscriber
    {
        string Filter { get; }

        Task HandleMessage(string topic, string payload);
    }
}
