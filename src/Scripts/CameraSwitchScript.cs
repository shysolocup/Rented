using Godot;
using System;
using CoolGame;

public partial class CameraSwitchScript : RbxScriptSource
{
	// Called when the node enters the scene tree for the first time.

	public CameraSwitchScript(RbxScript parent) : base(parent) {}

	public Camera3D camera1;
	public Camera3D camera2;

	public override async void _Ready()
	{
		GD.Print('a');

		GD.Print(ParentScript);

		// GD.Print(ParentScript);
		
		/*
		var test = await this.GetNodeAsync<Node>("test");
		test = GetNode(new NodePath("test"));
		
		GD.Print(test);
		
		camera1 = await this.GetNodeAsync<Camera3D>("%PlayerCamera");
		camera2 = await this.GetNodeAsync("%Cameras/Camera2") as Camera3D;

		camera1.MakeCurrent();
		*/
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
