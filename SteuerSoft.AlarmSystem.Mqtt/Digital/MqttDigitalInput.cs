﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.AlarmSystem.Core.Enums;
using SteuerSoft.AlarmSystem.Core.Interfaces;
using SteuerSoft.AlarmSystem.Core.Tools;
using SteuerSoft.AlarmSystem.Mqtt.Connector;

namespace SteuerSoft.AlarmSystem.Mqtt.Triggers
{
    internal class MqttDigitalInput : IDigitalInput, ISubscriber
    {
        public event EventHandler<DigitalInputStateEventArgs>? OnStateChanged;

        public bool State { get; private set; } = false;

        public string Filter { get; }

        private string _name;
        private string _offPayload;
        private string _onPayload;

        private bool _invert;

        private DigitalInDebouncer _deb = new DigitalInDebouncer();

        internal MqttDigitalInput(string name, string mqttfilter, string offPayload, string onPayload, bool invert)
        {
            _name = name;
            Filter = mqttfilter;
            _offPayload = offPayload;
            _onPayload = onPayload;
            _invert = invert;

            _deb.OnDebouncedState += _deb_OnDebouncedState; 
        }

        private void _deb_OnDebouncedState(object sender, bool state)
        {
            State = state;

            OnStateChanged?.Invoke(this, new DigitalInputStateEventArgs(_name, state));

        }

        public Task HandleMessage(string topic, string payload)
        {
            bool active = payload == _onPayload;
            bool inActive = payload == _offPayload;

            if (!active && !inActive)
            {
                // unknown payload
                return Task.CompletedTask;
            }

            bool state = (active || (inActive && _invert));

            _deb.Input(state);

            return Task.CompletedTask;
        }

    }
}
