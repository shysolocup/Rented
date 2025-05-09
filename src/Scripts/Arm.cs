using Godot;

public partial class Arm : Node3D
{
	[Export] public float Distance = 0f;
	[Export] public Camera3D Camera;

	[Export] public float PosSpeed = 1;
	[Export] public float RotSpeed = 5;
	[Export] public Vector3 Offset = new(0.3f, -1.1f, 0);


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

	public override void _PhysicsProcess(double delta)
	{
		if (Camera != null && IsInstanceValid(Camera)) {
			var TargetPos = Camera.GlobalTransform.Origin + Camera.GlobalTransform.Basis * Offset;

			Quaternion TargetQuat = Camera.GlobalTransform.Basis.GetRotationQuaternion();
			Quaternion CurrentQuat = GlobalTransform.Basis.GetRotationQuaternion();
			Quaternion NewQuat = CurrentQuat.Slerp(TargetQuat, RotSpeed * (float)delta);

			GlobalTransform = new Transform3D(new Basis(NewQuat), GlobalTransform.Origin.Lerp(TargetPos, PosSpeed * (float)delta));

		}
	}
}
