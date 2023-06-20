using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SteuerSoft.AlarmSystem.Core.Interfaces;
using SteuerSoft.AlarmSystem.Mqtt.Connector;

namespace SteuerSoft.AlarmSystem.Mqtt.Reporter
{
    public class DigitalInputStateReporter
    {
        private IPublisher _publisher;

        private string _topic;

        private Dictionary<object, string> _onText = new();
        private Dictionary<object, string> _offText = new();

        public DigitalInputStateReporter(IPublisher publisher, string topic)
        {
            _publisher = publisher;
            _topic = topic;
        }

        public void AddDigitalInput(IDigitalInput input, string onText, string offText)
        {
            input.OnStateChanged += Input_OnStateChanged;
            _onText.Add(input, onText);
            _offText.Add(input, offText);
        }

        private async void Input_OnStateChanged(object? sender, DigitalInputStateEventArgs e)
        {
            var state = new {Name = e.Name, State = e.NewState ? _onText[sender!] : _offText[sender!] };

            var json = JsonSerializer.Serialize(state);

            await _publisher.PublishMessage(_topic, json);
        }
    }
}
