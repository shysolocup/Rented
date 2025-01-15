using Godot;
using System;
using CoolGame;

[GlobalClass, Icon("res://addons/shylib/Images/Crosshair3D.png")]
public partial class Crosshair3D : StaticBody3D
{
	[Export] public StandardMaterial3D DefaultIcon;
	[Export] public float Distance = 1;
	[Export] public Camera3D Camera;
	[Export] public string Delay;
	
	public MeshInstance3D Icon;
	public Camera3D ViewportCamera;
	
	private SubViewportContainer CrosshairContainer;

	private float _delay = 0;

	public override void _Ready()
	{
		Delay = (Delay != null) ? Delay : "1/1";

		string[] spl = Delay.Split("/");
		_delay = float.Parse(spl[0]) / float.Parse(spl[1]);

		Camera = (Camera != null) ? Camera : GetNode<Camera3D>("%PlayerCamera");
		Icon = GetNode<MeshInstance3D>("./Icon");
		ViewportCamera = GetNodeOrNull<Camera3D>("../ViewportCamera");
		CrosshairContainer = GetParent().GetParent<SubViewportContainer>();
	}

	public override void _Process(double delta)
	{
		Transform3D Global = Camera.GlobalTransform;
		Vector3 Origin = Global.Origin;

		Vector3 ForwardVector = Global.Basis.Z;
		Vector3 LeftVector = Global.Basis.X;
		Vector3 UpVector = Global.Basis.Y;

		double noise = Mathf.Clamp(Game.Noise / 400, 0, 1);

		float randY = (float)GD.RandRange(-noise, noise);
		float randX = (float)GD.RandRange(-noise, noise);

		float noiseMod = 1 - (float)Mathf.Clamp(Game.Noise / 100, 0, 1);

		float A = CrosshairContainer.Modulate.R;

		// A = A + (B - A) * t

		float alpha = 1.25f - (float)Mathf.Clamp(A + (noiseMod - A) * 1/1.5f, 0, 1); // 10 noise is 0.1 alpha

		float rgb = A + (noiseMod - A) * 1/1.5f; // 10 noise is 0.9 red

		Distance += (20-(Game.Noise * 0.1f + 5) - Distance) * 1/1.5f; // 10 noise is 6 meters distance

		/*
			should always be 255 for red
			as noise rises to 100 it'll turn the crosshair red and make it shake rapdily
			as noise rises it'll also make the crosshair more visible increasing it's size and alpha
		*/
		CrosshairContainer.Modulate = new Color(1, rgb, rgb, alpha);

		Origin += ForwardVector * -Distance;
		Origin += LeftVector * randX;
		Origin += UpVector * randY;

		Transform3D IconTransform = new Transform3D(LeftVector, UpVector, ForwardVector, Origin);
	
		Icon.GlobalTransform = Icon.GlobalTransform.InterpolateWith(IconTransform, _delay);

		if (ViewportCamera != null) ViewportCamera.GlobalTransform = Global;
	}
}
