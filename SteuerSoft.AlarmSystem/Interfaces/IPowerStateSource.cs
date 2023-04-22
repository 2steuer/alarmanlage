namespace SteuerSoft.AlarmSystem.Interfaces;

public class PowerStateEventArgs : EventArgs
{
    public bool PowerState { get; set; }

    public PowerStateEventArgs(bool powerState)
    {
        PowerState = powerState;
    }
}

public interface IPowerStateSource
{
    event EventHandler<PowerStateEventArgs> PowerStateChanged;

    string Name { get; }
}