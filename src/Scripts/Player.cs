using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using CoolGame;


public partial class Player : CharacterBody3D
{

	[ExportCategory("Player")]
	[Export(PropertyHint.Range, "1,35,1")] public float speed = 10; // m/s

	[Export(PropertyHint.Range, "1,35,1")] public float base_walk_speed = 3; // m/s
	
	[Export(PropertyHint.Range, "1,35,1")] public float walk_speed = 3; // m/s
	
	[Export(PropertyHint.Range, "1,35,1")] public float sprint_speed = 5; // m/s

	[Export(PropertyHint.Range, "10,400,1")] public float acceleration = 100; // m/s^2

	[Export(PropertyHint.Range, "0.1,3.0,0.1")] public float jump_height = 0.5f; // m

	[Export(PropertyHint.Range, "0.1,3.0,0.1,or_greater")] public float camera_sens = 1;

	[Export] public bool tabbed_in = true;
	[Export] public bool controllable = true;

	public bool _jumping = false;
	public bool _sprinting = false;
	public bool _crouching = false;

	[Export] public bool jumping {
		get { return _jumping; }
		set { _jumping = value; changed("jumping", value); }
	}

	[Export] public bool sprinting {
		get { return _sprinting; }
		set { _sprinting = value; changed("sprinting", value); }
	}

	[Export] public bool crouching {
		get { return _crouching; }
		set { _crouching = value; changed("crouching", value); }
	}
	
	[Export] public bool mouse_captured = false;

	private Vector2 move_dir; // Input direction for movement
	private Vector2 look_dir; // Input direction for look/aim

	private Vector3 walk_vel; // Walking velocity 
	private Vector3 grav_vel; // Gravity velocity 
	private Vector3 jump_vel; // Jumping velocity

	public Tween speedIn;
	public Tween speedOut;

	public Tween tiltLeft;
	public Tween tiltRight;
	public Tween tiltBack;

	[Export] public float base_fov;
	[Export] public float crouch_speed = 1;
	
	public Tween crouchIn;
	public Tween crouchOut;

	[Export] public float tiltRot = 2;

	private Camera3D camera;

	private Variant nullvar = new Variant();

	[Export] public bool ActionCooldown = false;

	[Export] public float sprintFovMod = 15;

	[Export] public float crouchFovMod = -20;

	[Export] public Panel console;
	

	public override void _Ready()
	{	
		camera = GetNode<Camera3D>("%PlayerCamera");

		base_fov = camera.GetFov();
		capture_mouse();

		GD.Print(this.Get("camera"));

		FuckSpeed("speedIn", sprintFovMod, sprint_speed, 1.5f);
		FuckSpeed("speedOut", 0, base_walk_speed, 0.5f);

		CrouchEffect("crouchIn", crouchFovMod, crouch_speed, 0.5f);
		CrouchEffect("crouchOut", 0, base_walk_speed, 1);

		// console = GetTree().
	}


	// values handler for crouching and sprinting so when the value is changed it automatically updates
	private void changed(string property, Variant value)
	{
		// crouching
		if (property == "crouching" && (bool)value == true) {
			if (speedIn.IsRunning()) speedIn.Stop();
			if (speedOut.IsRunning()) speedOut.Stop();
			if (crouchOut.IsRunning()) crouchOut.Stop();

			if (!crouchIn.IsRunning()) crouchIn.Play();
		}
		else if (property == "crouching" && (bool)value == false) {
			if (crouchIn.IsRunning()) crouchIn.Stop();
			if (!crouchOut.IsRunning()) crouchOut.Play();
		}

		// sprinting
		else if (property == "sprinting" && (bool)value == true) {
			if (crouchIn.IsRunning()) crouchIn.Stop();
			if (crouchOut.IsRunning()) crouchOut.Stop();

			if (speedOut.IsRunning()) speedOut.Stop();
			if (!speedIn.IsRunning()) speedIn.Play();
		}
		else if (property == "sprinting" && (bool)value == false) {
			if (speedIn.IsRunning()) speedIn.Stop();
			if (!speedOut.IsRunning()) speedOut.Play();
		}
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWMWindowFocusIn) {
			tabbed_in = true;
		}
		else if (what == NotificationWMWindowFocusOut) {
			tabbed_in = false;

			crouching = false;
			sprinting = false;

			if (crouchIn != null && crouchIn.IsRunning()) crouchIn.Stop();
			if (crouchOut != null && crouchOut.IsRunning()) crouchOut.Stop();
			
			if (speedIn != null && speedIn.IsRunning()) speedIn.Stop();
			if (speedOut != null && speedOut.IsRunning()) speedOut.Stop();

			if (tiltRight != null && tiltRight.IsRunning()) tiltRight.Stop();
			if (tiltBack != null && tiltBack.IsRunning()) tiltBack.Stop();
			if (tiltLeft != null && tiltLeft.IsRunning()) tiltLeft.Stop();
		}
	}

	public override void _Process(double delta)
	{
		if (camera != null) camera.Position = Position;
	}

	public override void _PhysicsProcess(double delta)
	{


		if (!controllable) return;
		float d = (float)delta;
		if (mouse_captured) _handle_joypad_camera_rotation(d);
		Velocity = _move(d) + _gravity(d) + _jump(d);
		MoveAndSlide();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured) {
			var mouse = @event as InputEventMouseMotion;
			look_dir = mouse.Relative * 0.001f;

			if (mouse_captured) _rotate_camera();
		}

		if (!controllable) return;

		if (Input.IsActionJustPressed("MoveLeft")) {
			Tilt("tiltLeft", tiltRot, 0.2f).Play();
		}

		if (!Input.IsActionPressed("MoveRight") && !Input.IsActionPressed("MoveLeft")) {
			Tilt("tiltBack", 0, 0.2f).Play();
		}

		if (Input.IsActionJustPressed("MoveRight")) {
			Tilt("tiltRight", -tiltRot, 0.2f).Play();
		}
		
		if (!ActionCooldown && !sprinting && !crouching && Input.IsActionPressed("Sprint") && Input.IsActionPressed("MoveForward")) {
			sprinting = true;
		}
		else if (sprinting && (Input.IsActionJustReleased("Sprint") || Input.IsActionJustReleased("MoveForward"))) {
			sprinting = false;

			ActionCooldown = true;
			SceneTreeTimer timer = GetTree().CreateTimer(0.25f);
			timer.Timeout += () => ActionCooldown = false;
		}

		if (!ActionCooldown && !sprinting && !crouching && IsOnFloor() && Input.IsActionPressed("Crouch")) {
			crouching = true;
		}
		else if (crouching && Input.IsActionJustReleased("Crouch")) {
			crouching = false;

			ActionCooldown = true;
			SceneTreeTimer timer = GetTree().CreateTimer(0.25f);
			timer.Timeout += () => ActionCooldown = false;
		}

		if (Input.IsActionPressed("Jump") && !crouching) {
			jumping = true;
		}
		// if (Input.IsActionJustPressed("Exit")) GetTree().Quit();
	}

	private void capture_mouse()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		mouse_captured = true;
	}

	private void release_mouse()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
		mouse_captured = false;
	}

	private void _rotate_camera(float sens_mod = 1.0f)
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
		grav_vel = IsOnFloor() ? Vector3.Zero : grav_vel.MoveToward(new Vector3(0, Velocity.Y - Game.Gravity, 0), Game.Gravity * delta);
		return grav_vel;
	}

	public Vector3 _jump(float delta)
	{
		if (jumping) {
			if (jumping) {
				jump_vel = IsOnFloor() ? new Vector3(0, Mathf.Sqrt(4 * jump_height * Game.Gravity), 0) : jump_vel;
				jumping = false;

				return jump_vel;
			}
		}

		jump_vel = IsOnFloor() ? Vector3.Zero : jump_vel.MoveToward(Vector3.Zero, Game.Gravity * delta);
		return jump_vel;
	}

	public Tween FuckSpeed(string name, float zoommod = 0, float speed = 0, float time = 0)
	{
		Tween t = (Tween)Get(name);

		if (t != null) {
			t.Kill();
			Set(name, nullvar);
		};

		t = CreateTween();
		Set(name, t);

		t.Parallel().TweenProperty(this, new NodePath("walk_speed"), speed, time);
		t.Parallel().TweenProperty(camera, "fov", base_fov+zoommod, time);
		t.Finished += () => FuckSpeed(name, zoommod, speed, time);

		t.Stop();

		return t;
	}

	public Tween CrouchEffect(string name, float zoommod = 0, float speed = 0, float scale = 0)
	{
		Tween t = (Tween)Get(name);

		if (t != null) {
			t.Kill();
			Set(name, nullvar);
		};

		t = CreateTween();
		Set(name, t);

		t.SetTrans(Tween.TransitionType.Cubic);

		t.Parallel().TweenProperty(this, new NodePath("walk_speed"), speed, 0.5f);
		t.Parallel().TweenProperty(camera, "fov", base_fov+zoommod, 0.5f);
		t.Parallel().TweenProperty(this, "scale:y", scale, 0.7f);

		t.Finished += () => CrouchEffect(name, zoommod, speed, scale);

		t.Stop();

		return t;
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

		t.Stop();

		return t;
	}
}
