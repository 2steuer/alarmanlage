using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.AlarmSystem.Core.Interfaces
{
    public interface ISequenceBuilder
    {
        ISequenceBuilder AddAction(ISequenceEntry entry);
    }
}
