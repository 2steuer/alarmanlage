namespace SteuerSoft.AlarmSystem;

public enum State
{
    Off,
    Arming,
    Idle,
    PreAlarm,
    Alarm
}

enum Triggers
{
    TogglePower,
    PowerOn,
    PowerOff,
    ArmingDelayElapsed,
    PreAlarmDelayElapsed,
    Alarm,
    ImmediateAlarm
}

public enum TriggerType
{
    Alarm,
    ImmediateAlarm
}