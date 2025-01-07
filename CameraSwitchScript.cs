using Godot;
using System;

public partial class CameraSwitchScript : Node
{
	// Called when the node enters the scene tree for the first time.

	public Camera3D camera1;
	public Camera3D camera2;

	public override void _Ready()
	{
		Node3D parent = GetParent() as Node3D;

		camera1 = parent.GetNode(new NodePath("Camera")) as Camera3D;
		camera2 = parent.GetNode(new NodePath("Camera2")) as Camera3D;

		camera1.MakeCurrent();
	}


	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("SwitchCamera") && camera1 != null && camera2 != null) {
			if (camera1.Current) {
				camera2.MakeCurrent();
			}
			else if (camera2.Current) {
				camera1.MakeCurrent();
			}
		}
	}
}
