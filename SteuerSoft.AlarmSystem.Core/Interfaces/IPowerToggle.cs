namespace SteuerSoft.AlarmSystem.Core.Interfaces;

public interface IPowerToggle
{
    event EventHandler TogglePower;

    string Name { get; }
}