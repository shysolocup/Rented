using Godot;
using System;
using System.Linq;

public partial class Player : CharacterBody3D
{

	[ExportCategory("Player")]
	[Export(PropertyHint.Range, "1,35,1")]
	public float speed = 10; // m/s

	[Export(PropertyHint.Range, "1,35,1")]
	public float base_walk_speed = 3; // m/s
	
	[Export(PropertyHint.Range, "1,35,1")]
	public float walk_speed = 3; // m/s
	
	[Export(PropertyHint.Range, "1,35,1")]
	public float sprint_speed = 10; // m/s

	[Export(PropertyHint.Range, "10,400,1")]
	public float acceleration = 100; // m/s^2

	[Export(PropertyHint.Range, "0.1,3.0,0.1")]
	public float jump_height = 1; // m

	[Export(PropertyHint.Range, "0.1,3.0,0.1,or_greater")]
	public float camera_sens = 1;


	public bool jumping = false;
	public bool sprinting = false;
	public bool crouching = false;
	public bool mouse_captured = false;

	public float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

	public Vector2 move_dir; // Input direction for movement
	public Vector2 look_dir; // Input direction for look/aim

	public Vector3 walk_vel; // Walking velocity 
	public Vector3 grav_vel; // Gravity velocity 
	public Vector3 jump_vel; // Jumping velocity

	public Tween speedIn;
	public Tween speedOut;

	public Tween tiltLeft;
	public Tween tiltRight;
	public Tween tiltBack;

	public float base_fov;
	
	public Tween crouchIn;
	public Tween crouchOut; 

	public float tiltRot = 2;

	public Camera3D camera;

	public Variant nullvar = new Variant();


	public Tween FuckSpeed(string name, float speed = 0)
	{
		Tween t = (Tween)Get(name);

		if (t != null) {
			t.Kill();
			Set(name, nullvar);
		};

		t = CreateTween();
		Set(name, t);

		t.TweenProperty(this, new NodePath("walk_speed"), speed, 1);
		t.Finished += () => FuckSpeed(name, speed);

		return t;
	}

	public Tween CrouchEffect(string name, float zoom = 0, float speed = 0)
	{
		Tween t = (Tween)Get(name);

		if (t != null) {
			t.Kill();
			Set(name, nullvar);
		};

		t = CreateTween();
		Set(name, t);

		t.TweenProperty(this, new NodePath("walk_speed"), speed, 1);
		t.TweenProperty(camera, "fov", zoom, 1);

		t.Finished += () => CrouchEffect(name, speed, zoom);

		return t;
	}

	public override void _Ready()
	{
		camera = GetParent().GetNode(new NodePath("Camera")) as Camera3D;
		base_fov = camera.GetFov();
		capture_mouse();

		FuckSpeed("speedIn", sprint_speed);
		FuckSpeed("speedOut", base_walk_speed);

		CrouchEffect("crouchIn", base_fov+10);
		CrouchEffect("crouchIn", base_fov);
	}

	public override void _Process(double delta)
	{
		if (camera != null) camera.Position = Position;
	}

	public override void _PhysicsProcess(double delta)
	{
		float d = (float)delta;
		if (mouse_captured) _handle_joypad_camera_rotation(d);
		Velocity = _move(d) + _gravity(d) + _jump(d);
		MoveAndSlide();
	}

	public Tween Tilt(string name, float degrees = 0, float time = 0.2f)
	{
		if (tiltRight != null) { tiltRight.Kill(); tiltRight = null; }
		if (tiltBack != null) { tiltBack.Kill(); tiltBack = null; }
		if (tiltLeft != null) { tiltLeft.Kill(); tiltLeft = null; }

		Tween t = CreateTween();
		Set(name, t);
		t.SetTrans(Tween.TransitionType.Quad);
		t.TweenProperty(camera, "rotation_degrees:z", degrees, time);
		t.Finished += () => t.Kill(); Set(name, nullvar);

		return t;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured) {
			var mouse = @event as InputEventMouseMotion;
			look_dir = mouse.Relative * 0.001f;

			if (mouse_captured) _rotate_camera();
		}

		if (Input.IsActionJustPressed("MoveLeft")) {
			Tilt("tiltLeft", tiltRot, 0.2f).Play();
		}

		if (!Input.IsActionPressed("MoveRight") && !Input.IsActionPressed("MoveLeft")) {
			Tilt("tiltBack", 0, 0.2f).Play();
		}

		if (Input.IsActionJustPressed("MoveRight")) {
			Tilt("tiltRight", -tiltRot, 0.2f).Play();
		}
		
		if (!sprinting && !crouching && Input.IsActionPressed("Sprint") && Input.IsActionPressed("MoveForward")) {
			sprinting = true;
			if (speedOut.IsRunning()) speedOut.Stop();
			if (!speedIn.IsRunning()) speedIn.Play();
		}
		else if (sprinting && (Input.IsActionJustReleased("Sprint") || Input.IsActionJustReleased("MoveForward"))) {
			sprinting = false;
			if (speedIn.IsRunning()) speedIn.Stop();
			if (!speedOut.IsRunning()) speedOut.Play();
		}

		if (!sprinting && !crouching && Input.IsActionPressed("Crouch")) {
			crouching = true;
			if (crouchOut.IsRunning()) crouchOut.Stop();
			if (!crouchIn.IsRunning()) crouchIn.Play();
		}
		else if (crouching && (Input.IsActionJustReleased("Crouch"))) {
			crouching = false;
			if (crouchIn.IsRunning()) crouchIn.Stop();
			if (!crouchOut.IsRunning()) crouchOut.Play();
		}

		if (Input.IsActionPressed("Jump")) jumping = true;
		// if (Input.IsActionJustPressed("Exit")) GetTree().Quit();
	}

	public void capture_mouse()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		mouse_captured = true;
	}

	public void release_mouse()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
		mouse_captured = false;
	}

	public void _rotate_camera(float sens_mod = 1.0f)
	{
		camera.RotateY( -(look_dir.X * camera_sens * sens_mod) );

		var x = Mathf.Clamp(camera.Rotation.X - look_dir.Y * camera_sens * sens_mod, -1.5f, 1.5f);

		camera.Rotation = new Vector3(x, camera.Rotation.Y, camera.Rotation.Z);
	}

	public void _handle_joypad_camera_rotation(float delta, float sens_mod = 10)
	{
		Vector2 joypad_dir = Input.GetVector("LookLeft", "LookRight", "LookUp", "LookDown");
		if (joypad_dir.Length() > 0) {
			look_dir += joypad_dir * delta;
			
			_rotate_camera(sens_mod);
			
			look_dir = Vector2.Zero;
		}
	}

	public Vector3 _move(float delta)
	{
		move_dir = Input.GetVector("MoveLeft", "MoveRight", "MoveForward", "MoveBack");

		Vector3 _forward = camera.GlobalTransform.Basis * new Vector3(move_dir.X, 0, move_dir.Y);
		Vector3 walk_dir = new Vector3(_forward.X, 0, _forward.Z).Normalized();
		
		walk_vel = walk_vel.MoveToward(walk_dir * walk_speed * move_dir.Length(), acceleration * delta);
		return walk_vel;
	}

	public Vector3 _gravity(float delta)
	{
		grav_vel = IsOnFloor() ? Vector3.Zero : grav_vel.MoveToward(new Vector3(0, Velocity.Y - gravity, 0), gravity * delta);
		return grav_vel;
	}

	public Vector3 _jump(float delta)
	{
		if (jumping) {
			if (jumping) {
				jump_vel = IsOnFloor() ? new Vector3(0, Mathf.Sqrt(4 * jump_height * gravity), 0) : jump_vel;
				jumping = false;

				return jump_vel;
			}
		}

		jump_vel = IsOnFloor() ? Vector3.Zero : jump_vel.MoveToward(Vector3.Zero, gravity * delta);
		return jump_vel;
	}
}
