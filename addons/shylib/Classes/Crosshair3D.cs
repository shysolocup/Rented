using Godot;
using System;
using CoolGame;

[GlobalClass, Icon("res://addons/shylib/Images/Crosshair3D.png")]
public partial class Crosshair3D : StaticBody3D
{
	[Export] public StandardMaterial3D DefaultIcon;
	[Export] public StandardMaterial3D DefaultHoverIcon;
	[Export] public StandardMaterial3D LockedIcon;
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

		double Noise = Mathf.Clamp(Game.Instance.Noise / 400, 0, 1);

		float ShakeY = (float)GD.RandRange(-Noise, Noise);
		float ShakeX = (float)GD.RandRange(-Noise, Noise);

		float NoiseMod = 1 - (float)Mathf.Clamp(Game.Instance.Noise / 100, 0, 1);

		float A = CrosshairContainer.Modulate.R;

		// A = A + (B - A) * t

		float Alpha = 1.25f - (float)Mathf.Clamp(A + (NoiseMod - A) * 1 / 3f, 0, 1); // 10 noise is 0.1 alpha

		float GB = A + (NoiseMod - A) * 1 / 3f; // 10 noise is 0.9 green and blue

		Distance += (20 - (Game.Instance.Noise * 0.1f + 5) - Distance) * 1 / 3f; // 10 noise is 6 meters distance

		/*
			should always be 255 for red
			as noise rises to 100 it'll turn the crosshair red and make it shake rapdily
			as noise rises it'll also make the crosshair more visible increasing it's size and alpha
		*/
		CrosshairContainer.Modulate = new Color(1, GB, GB, Alpha);

		Origin += ForwardVector * -Distance;
		Origin += LeftVector * ShakeX;
		Origin += UpVector * ShakeY;

		Transform3D IconTransform = new Transform3D(LeftVector, UpVector, ForwardVector, Origin);

		Icon.GlobalTransform = Icon.GlobalTransform.InterpolateWith(IconTransform, _delay);

		if (ViewportCamera != null) ViewportCamera.GlobalTransform = Global;
	}
}
