using Godot;

public partial class Arm : Node3D
{
	[Export] public Camera3D Camera;

	public float PosSpeed = 5;
	public float RotSpeed = 15;
	public Vector3 Offset = new(0.3f, -1.3f, 0);

	public override void _Ready()
	{
		Camera ??= this.GetGameNode<Camera3D>("%PlayerCamera");
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
