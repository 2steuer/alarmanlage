﻿namespace SteuerSoft.AlarmSystem.Core.Interfaces;

public interface ISequenceEntry
{
    Task Execute(CancellationToken ctx);

    Task Reset(CancellationToken ctx);
}