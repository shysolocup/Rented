using Godot;

[Tool]
[GlobalClass, Icon("uid://b4blw6r8ym2hk")]
public partial class Flashlight : Item 
{
    static private bool broken = false;
    static private bool on = false;

    [Export] public bool On
    {
        get => !broken && on;
        set => on = value;
    }

    // will play an animation for the flashlight breaking and fixing
    [Export] public bool Broken {
        get => broken;
        set {
            if (value) On = false;
            broken = value;
        }
    }

    public override void _Ready()
    {
        
    }
}