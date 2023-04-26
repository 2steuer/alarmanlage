namespace SteuerSoft.AlarmSystem.Core.Sequences.Actions.Base;

public interface ISwitch
{
    Task Switch(bool on, CancellationToken ctx = default);
}