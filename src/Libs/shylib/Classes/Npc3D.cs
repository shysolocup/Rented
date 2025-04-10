using Godot;
using System;

[GlobalClass]
public partial class Npc3D : InteractObject3D
{

    [Export] new public string Line = "convo_default";

    public override void _Ready()
    {
        base._Ready();
    }
}