using Godot;

[Tool]
[GlobalClass]
public partial class Flashlight : Item 
{
    private bool broken = false;

    [Export] public bool On = false;

    // will play an animation for the flashlight breaking and fixing
    [Export] public bool Broken {
        get => broken;
        set {
            broken = value;
        }
    }

    public override void _Ready()
    {
        
    }
}