using Godot;
using System;

public partial class TriggerTest : Node
{
    private Trigger3D Trigger;

	public override void _Ready()
	{
		base._Ready();

        Trigger = GetParent<Trigger3D>();

        Trigger.Touched += (Node toucher) => {
            GD.Print(toucher);
        };

        Trigger.TouchEnded += (Node toucher) => {
            GD.Print(toucher);
        };
	}
}
