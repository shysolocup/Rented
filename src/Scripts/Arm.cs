using Godot;
using System;
using CoolGame;

public partial class Arm : Node3D
{
	[Export] public float Distance = 0f;
	[Export] public Camera3D Camera;


	private string delay = "1/7";

	[Export] public string Delay {
		get => delay;
		set {
			delay = value;
			string[] spl = Delay.Split("/");
			FloatingDelay = float.Parse(spl[0]) / float.Parse(spl[1]);
		}
	}

	private float FloatingDelay = 0;

	public override void _Ready()
	{
		Delay ??= "1/1";

		string[] spl = Delay.Split("/");
		FloatingDelay = float.Parse(spl[0]) / float.Parse(spl[1]);

		Camera ??= GetNode<Camera3D>("../%PlayerCamera");
	}

	public override void _Process(double delta)
	{
		if (Camera != null && IsInstanceValid(Camera)) {
			Transform3D CameraGlobal = Camera.GlobalTransform;
			Basis CameraBasis = CameraGlobal.Basis;
			Vector3 Origin = CameraGlobal.Origin;

			Vector3 ForwardVector = CameraBasis.Z;
			Vector3 LeftVector = CameraBasis.X;
			Vector3 UpVector = CameraBasis.Y;

			Origin += ForwardVector * -Distance;
			Origin += LeftVector * 0.3f;
			Origin += UpVector * -1f;

			Origin = GlobalTransform.Origin.Lerp(Origin, this.FactorDelta(1/2f, delta));

			Basis ArmBasis = new(LeftVector, UpVector, ForwardVector);
			ArmBasis = ArmBasis.Scaled(new Vector3(0.8f, 0.8f, 0.8f));

			Transform3D ArmTransform = new(ArmBasis, Origin);

			GlobalTransform = GlobalTransform.InterpolateWith(ArmTransform, this.FactorDelta(FloatingDelay, delta));	
		}
	}
}
