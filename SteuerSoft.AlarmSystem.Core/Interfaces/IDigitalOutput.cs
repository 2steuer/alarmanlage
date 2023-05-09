using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.AlarmSystem.Core.Interfaces
{
    public interface IDigitalOutput
    {
        string Name { get; }
        Task Set(bool on, CancellationToken ctx = default);

        Task SetDefault(CancellationToken ctx = default);
    }
}
