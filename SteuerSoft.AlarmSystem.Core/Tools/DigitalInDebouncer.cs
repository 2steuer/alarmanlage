using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.AlarmSystem.Core.Tools
{
    /// <summary>
    /// Used to debounce digital inputs.
    /// </summary>
    public class DigitalInDebouncer
    {
        public event Action<object, bool>? OnDebouncedState; 

        public TimeSpan DebounceTime { get; set; } = TimeSpan.FromSeconds(1);

        private object _lock = new object();
        private bool _debounceRunning = false;

        private bool _currentState = false;

        public DigitalInDebouncer(bool initState = false)
        {
            _currentState = initState;
        }

        public void Input(bool inputState)
        {
            if (inputState == _currentState)
            {
                return;
            }

            _currentState = inputState;

            lock (_lock)
            {
                if (_debounceRunning)
                {
                    return;
                }

                _debounceRunning = true;
            }

            ThreadPool.QueueUserWorkItem(state => Debouncer(inputState));
        }

        private void Debouncer(bool targetState)
        {
            Thread.Sleep(DebounceTime);

            if (_currentState == targetState)
            {
                OnDebouncedState?.Invoke(this, targetState);
            }

            lock (_lock)
            {
                _debounceRunning = false;
            }
        }


    }
}
