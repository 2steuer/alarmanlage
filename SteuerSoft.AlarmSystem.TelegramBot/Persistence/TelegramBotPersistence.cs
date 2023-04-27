using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Infrastructure;
using Appccelerate.StateMachine.Persistence;
using Newtonsoft.Json;

namespace SteuerSoft.AlarmSystem.TelegramBot.Persistence
{
    internal class TelegramBotPersistence : IAsyncStateMachineSaver<BotInstanceState, BotInstanceTrigger>, IAsyncStateMachineLoader<BotInstanceState, BotInstanceTrigger>
    {
        private string _filePath;

        public TelegramBotPersistence(string basePath, long chatId)
        {
            _filePath = Path.Combine(basePath, $"{chatId}.json");
        }

        private PersistenceObject LoadSettings()
        {
            if (!File.Exists(_filePath))
            {
                // create new
                SaveSettings(new PersistenceObject());
            }

            var json = File.ReadAllText(_filePath, Encoding.UTF8);
            return JsonConvert.DeserializeObject<PersistenceObject>(json);
        }

        private void SaveSettings(PersistenceObject obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            File.WriteAllText(_filePath, json, Encoding.UTF8);
        }

        public Task SaveCurrentState(IInitializable<BotInstanceState> currentStateId)
        {

            var obj = LoadSettings();
            obj.CurrentState = currentStateId.IsInitialized ? currentStateId.ExtractOrThrow() : null;
            SaveSettings(obj);

            return Task.CompletedTask;
        }

        public Task SaveHistoryStates(IReadOnlyDictionary<BotInstanceState, BotInstanceState> historyStates)
        {
            var obj = LoadSettings();
            obj.HistoryStates = new Dictionary<BotInstanceState, BotInstanceState>(historyStates);
            SaveSettings(obj);

            return Task.CompletedTask;
            
        }

        public Task SaveEvents(IReadOnlyCollection<EventInformation<BotInstanceTrigger>> events)
        {
            return Task.CompletedTask; // we do not support this
        }

        public Task SavePriorityEvents(IReadOnlyCollection<EventInformation<BotInstanceTrigger>> priorityEvents)
        {
            return Task.CompletedTask; // we do not support this
        }

        public Task<IInitializable<BotInstanceState>> LoadCurrentState()
        {
            var obj = LoadSettings();

            return Task.FromResult<IInitializable<BotInstanceState>>(!obj.CurrentState.HasValue
                ? Initializable<BotInstanceState>.UnInitialized()
                : Initializable<BotInstanceState>.Initialized(obj.CurrentState.Value)
            );
        }

        public Task<IReadOnlyDictionary<BotInstanceState, BotInstanceState>> LoadHistoryStates()
        {
            var obj = LoadSettings();

            return Task.FromResult<IReadOnlyDictionary<BotInstanceState, BotInstanceState>>(obj.HistoryStates);
        }

        public Task<IReadOnlyCollection<EventInformation<BotInstanceTrigger>>> LoadEvents()
        {
            // return empty list
            return Task.FromResult<IReadOnlyCollection<EventInformation<BotInstanceTrigger>>>(
                    Array.Empty<EventInformation<BotInstanceTrigger>>()
                );

        }

        public Task<IReadOnlyCollection<EventInformation<BotInstanceTrigger>>> LoadPriorityEvents()
        {
            // return empty list
            return Task.FromResult<IReadOnlyCollection<EventInformation<BotInstanceTrigger>>>(
                    Array.Empty<EventInformation<BotInstanceTrigger>>()
                );
        }
    }
}
