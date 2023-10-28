using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.AlarmSystem.Core.Interfaces;

namespace SteuerSoft.AlarmSystem.Core.Sequences.Actions
{
    public class RepeatAction : ISequenceEntry, ISequenceBuilder
    {
        private int _repeatCount;
        private List<ISequenceEntry> _entries = new();

        public RepeatAction(int count)
        {
            _repeatCount = count;
        }

        public async Task Execute(CancellationToken ctx)
        {
            for (int i = 0; i < _repeatCount; i++)
            {
                foreach (var sequenceEntry in _entries)
                {
                    await sequenceEntry.Execute(ctx);
                }
            }
        }

        public async Task Reset(CancellationToken ctx)
        {
            foreach (var sequenceEntry in _entries)
            {
                await sequenceEntry.Reset(ctx);
            }
        }

        public ISequenceBuilder AddAction(ISequenceEntry entry)
        {
            _entries.Add(entry);

            return this;
        }
    }
}
