using Godot;
using System;

public partial class ViewportModelCamera : Camera3D
{
	private Camera3D camera;

	public override void _Ready()
	{
		camera = this.GetGameNode<Camera3D>("%PlayerCamera");
	}

	public override void _Process(double delta)
	{
		if (camera is not null)
		{
			GlobalTransform = camera.GlobalTransform;
		}
	}
}
