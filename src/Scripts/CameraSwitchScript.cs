using Godot;
using System;
using CoolGame;

public partial class CameraSwitchScript : RbxScriptSource
{
	public CameraSwitchScript(RbxScript parent) : base(parent) {}

	public Camera3D camera1;
	public Camera3D camera2;

	public override void _Ready()
	{
		camera1 = GetParent().GetNode<Camera3D>("%PlayerCamera");
		camera2 = GetParent().GetNode<Camera3D>("%Cameras/Camera2");

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
