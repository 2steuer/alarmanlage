namespace SteuerSoft.AlarmSystem;

enum State
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