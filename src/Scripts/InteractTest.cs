using Godot;
using System;

public partial class InteractTest : Node3D
{
	public InteractObject3D obj;

	public override void _Ready() {
		obj = GetParent<InteractObject3D>();

		obj.HoverChanged += Hover;
		obj.PressChanged += Press;
	}

	public void Hover()
	{
		if (obj.Hovering) {
			GD.Print("Started hovering");
		}
		else {
			GD.Print("Stopped hovering");
		}
	}

	public void Press()
	{
		if (obj.Pressed) {
			GD.Print("Started pressing");
		}
		else {
			GD.Print("Stopped pressing");
		}
	}
}
