using Godot;
using System;
using CoolGame;

public partial class Arm : Node3D
{
	[Export] public float Distance = 0f;
	[Export] public Camera3D Camera;
	[Export] public string Delay = "1/7";

	private float _delay = 0;

	public override void _Ready()
	{
		Delay ??= "1/1";

		string[] spl = Delay.Split("/");
		_delay = float.Parse(spl[0]) / float.Parse(spl[1]);

		Camera ??= GetNode<Camera3D>("%PlayerCamera");
	}

	public override void _Process(double delta)
	{
		Transform3D CameraGlobal = Camera.GlobalTransform;
		Basis CameraBasis = CameraGlobal.Basis;
		Vector3 Origin = CameraGlobal.Origin;

		Vector3 ForwardVector = CameraBasis.Z;
		Vector3 LeftVector = CameraBasis.X;
		Vector3 UpVector = CameraBasis.Y;

		Origin += ForwardVector * -Distance;
		Origin += LeftVector * -1;
		Origin += UpVector * -1;

		Basis ArmBasis = new(LeftVector, UpVector, ForwardVector);
		ArmBasis = ArmBasis.Scaled(new Vector3(0.6f, 0.6f, 0.6f));

		Transform3D ArmTransform = new(ArmBasis, Origin);

		GlobalTransform = ArmTransform; // GlobalTransform.InterpolateWith(ArmTransform, this.FactorDelta(_delay, delta));
	}
}
