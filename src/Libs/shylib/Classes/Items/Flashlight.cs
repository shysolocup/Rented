using Godot;

[Tool]
[GlobalClass]
public partial class Flashlight : Item 
{
    static private bool broken = false;

    static public bool On = false;

    // will play an animation for the flashlight breaking and fixing
    static public bool Broken {
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