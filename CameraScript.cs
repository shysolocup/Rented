using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class CameraScript : Node
{

	public CharacterBody3D player;
	public Camera3D camera;
	public Node3D pivot;

	public float t = 0;
	public float sens = 0.1f;

	public Vector3 offset = new Vector3(0, 0.5f, 0.5f);
	public Vector3 min = new Vector3(-70, -70, -70);
	public Vector3 max = new Vector3(70, 70, 70);


	public Node GetNodeChild( Node node, string name ) 
	{
		return node.GetNode(new NodePath(name));
	}


	public float DegToRad(float degrees)
	{
		return (Mathf.Pi/180) * degrees;
	}


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (!(bool)GetMeta("Enabled")) return;

		Input.MouseMode = Input.MouseModeEnum.Captured;

		player = GetParent() as CharacterBody3D;
		pivot = GetNodeChild(player, "Pivot") as Node3D;
		camera = GetNodeChild(player.GetParent(), "Camera") as Camera3D;
	}

/*
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (camera != null && player != null) {
			camera.Transform = player.Transform;
			GD.Print(pivot.Rotation);
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseModeEnum.Captured) {

			// GD.Print('a');

			var mouse = @event as InputEventMouseMotion;

			pivot.RotateX(DegToRad(mouse.Relative.Y * sens));
			player.RotateY(DegToRad(mouse.Relative.X * sens) * -1);

			camera.RotateY(-(mouse.Relative.X * sens));

			float x = (float)Math.Clamp(camera.Rotation.X - mouse.Relative.X * sens, -1.5, 1.5);

			camera.Rotation = new Vector3(x, camera.Rotation.Y, camera.Rotation.Z);
		}
   	}
	*/
}
