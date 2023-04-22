namespace SteuerSoft.AlarmSystem.Interfaces;

public interface IPowerToggle
{
    event EventHandler TogglePower;

    string Name { get; }
}