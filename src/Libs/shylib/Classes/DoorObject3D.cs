using System;
using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class DoorObject3D : InteractObject3D
{
    private bool _opened = false;
    [Export] public bool Opened {
        get {
            return _opened;
        }

        set {
            if (value != _opened) _opened = value;
        }
    }

    [Export] public bool Locked = false;

    [Export] public Transform3D Goal = Transform3D.Identity;
    public Transform3D Origin;


    [ExportToolButton("Reset")] public Callable ResetButton => Callable.From(Reset);
    [ExportToolButton("Open")] public Callable OpenButton => Callable.From(Open);
    [ExportToolButton("Set Origin")] public Callable SetOriginButton => Callable.From(RefreshOrigin);


    public void RefreshOrigin() { Origin = Transform; }
    public void Open() { Opened ^= true; } 
    public void Reset() { Transform = Origin; } 


    public override void _PressChanged()
    {
        base._PressChanged();
    }

    public override void _Ready()
    {
        base._Ready();
        Origin = Transform;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}