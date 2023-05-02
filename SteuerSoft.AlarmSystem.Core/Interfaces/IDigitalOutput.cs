using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.AlarmSystem.Core.Interfaces
{
    internal interface IDigitalOutput
    {
        string Name { get; }
        Task Set(bool on);
    }
}
