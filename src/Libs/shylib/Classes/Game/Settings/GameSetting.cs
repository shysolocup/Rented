using Godot;

public partial class GameSetting : PanelContainer
{
    [Signal] public delegate void EnabledEventHandler();
    [Signal] public delegate void DisabledEventHandler();

    public virtual void _Enabled() { }
    public virtual void _Disabled() { }
}