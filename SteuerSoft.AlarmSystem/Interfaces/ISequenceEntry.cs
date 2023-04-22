namespace SteuerSoft.AlarmSystem.Interfaces;

public interface ISequenceEntry
{
    Task Execute(CancellationToken ctx);
}