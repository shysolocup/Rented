using Godot;
using Godot.Collections;
using System;
using CoolGame;
using System.Threading.Tasks;


public partial class Player : CharacterBody3D
{

	[ExportCategory("Player")]
	[Export(PropertyHint.Range, "1,35,1")] public float Speed = 10; // m/s

	[Export(PropertyHint.Range, "1,35,1")] public float BaseWalkSpeed = 3; // m/s
	
	[Export(PropertyHint.Range, "1,35,1")] public float WalkSpeed = 3; // m/s
	
	[Export(PropertyHint.Range, "1,35,1")] public float SprintSpeed = 5; // m/s

	[Export(PropertyHint.Range, "10,400,1")] public float Acceleration = 100; // m/s^2

	[Export(PropertyHint.Range, "0.1,3.0,0.1")] public float JumpHeight = 0.5f; // m

	[Export(PropertyHint.Range, "0.1,3.0,0.1,or_greater")] public float CameraSensitivity = 1;

	[Export] public bool TabbedIn = true;
	[Export] public bool Controllable = true;
	[Export] public bool CameraRotationControllable = true;
	[Export] public bool CameraPositionControllable = true;

	[Export ] public bool Jumping = false;
	[Export] public bool Sprinting = false;
	[Export] public bool Crouching = false;
	
	[Export] public bool MouseCaptured = false;

	private Vector2 MoveDirection; // Input direction for movement
	private Vector2 LookDirection; // Input direction for look/aim

	private Vector3 WalkVelocity; // Walking velocity 
	private Vector3 GravityVelocity; // Gravity velocity 
	private Vector3 JumpVelocity; // Jumping velocity

	[Export] public float BaseFov;
	[Export] public float CrouchSpeed = 1f;

	[Export] public float TiltRotation = 3;

	private Camera3D Camera;

	private Variant nullvar = new();
	
	public bool InDialog = false; 

	[Export] public bool ActionCooldown = false;

	[Export] public float SprintFovMod = 20;

	[Export] public float CrouchFovMod = -20;
	[Export] public float InteractFovMod = -15;

	[Export] public bool Tilt = true;
	
	public RayCast3D Raycast;
	public InteractObject3D Inter;

	[Export] public bool Walking = false;
	private bool WalkingEffecting = false;

	private float DefaultCameraOffset = 0.4f;
	private float CameraOffset;
	private CollisionShape3D Collision;
	private CollisionShape3D Collision2;
	private MeshInstance3D Mesh;
	

	public override void _Ready()
	{	
		Camera = GetNode<Camera3D>("%PlayerCamera");
		Raycast = GetNode<RayCast3D>("%InteractRay");

		Collision = GetNode<CollisionShape3D>("./Collision");
		Collision2 = GetNode<CollisionShape3D>("./CrouchCollision");
		Mesh = GetNode<MeshInstance3D>("./Mesh");

		CameraOffset = DefaultCameraOffset;

		BaseFov = Camera.GetFov();
		CaptureMouse();

		GD.Print(Get("camera"));

		// CrouchEffect("crouchIn", crouchFovMod, crouch_speed, 0.5f);
		// CrouchEffect("crouchOut", 0, base_walk_speed, 1);

		// console = GetTree().
	}

	public override void _Notification(int what)
	{

		if (what == NotificationWMWindowFocusIn) {
			TabbedIn = true;
		}
		else if (what == NotificationWMWindowFocusOut) {
			TabbedIn = false;

			Crouching = false;
			Sprinting = false;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// if (!controllable) return;
		
		float d = (float)delta;
		if (MouseCaptured) JoypadControls(d);

		Velocity = GetMove(d) + GetGravity(d) + GetJump(d);
		MoveAndSlide();
	}

	public override async void _Process(double delta)
	{
		if (Raycast.IsColliding()) {
			GodotObject result = Raycast.GetCollider();

			if (Inter != null && result != Inter) {
				Inter.Hovering = false;
				Inter = null;
			}

			if (result.GetType() == typeof(InteractObject3D)) {
				InteractObject3D collider = (InteractObject3D)result;
				Inter = collider;
				collider.Hovering = true;
			}
		}

		else if (Inter != null) {
			Inter.Hovering = false;
			Inter = null;
		}

		if (Camera != null) {

			if (CameraPositionControllable) {
				Camera.Position = new Vector3(Position.X, Position.Y + CameraOffset, Position.Z); 
			}
			Rotation = new Vector3(Rotation.X, Camera.Rotation.Y, Rotation.Z);

			// crouch effect
			if (Crouching) {
				SpeedEffect(CrouchFovMod, CrouchSpeed, -0.4f, true, 1/25f, delta);
			}
			if (Sprinting) {
				SpeedEffect(SprintFovMod, SprintSpeed, DefaultCameraOffset, false, 1/45f, delta);
			}
			else if (!Crouching && !Sprinting) {
				SpeedEffect( InDialog ? InteractFovMod : 0, BaseWalkSpeed, DefaultCameraOffset, false, 1/30f, delta);
			}

			// tilt effect
			if (Controllable && Input.IsActionPressed("MoveLeft")) {
				TiltEffect(TiltRotation, 1/12f, delta);
			}
			else if (Controllable && Input.IsActionPressed("MoveRight")) {
				TiltEffect(-TiltRotation, 1/12f, delta);
			}
			else if (Camera != null && Camera.RotationDegrees.Z != 0) {
				TiltEffect(0, 1/12f, delta);
			}


			// walk effect
			if (Walking) {
				await WalkEffect(delta);
			}
		}

		
	}


	public async void SnatchInteract(InteractObject3D obj) 
	{
		if (obj.Cooldown) {
			obj.Cooldown = false; return;
		}

		obj.Cooldown = true;
		Controllable = false;
		CameraRotationControllable = false;
		InDialog = true;

		Vector3 Direction = (obj.GlobalTransform.Origin - GlobalTransform.Origin).Normalized();
		Basis TargetBasis = Basis.LookingAt(Direction, Vector3.Up);
		Vector3 TargetRotation = TargetBasis.GetEuler();

		float time = (obj.GlobalRotation - Camera.GlobalRotation).Length() / 1.3f;

		Tween tween = Camera.CreateTween();
		tween.Finished += () => tween.Dispose();

		tween.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);

		tween.TweenProperty(Camera, "rotation", TargetRotation, time);

		tween.Play();
			
		DialogueData data = GetNode<DialogueData>("%DialogueData");
		
		await data.Play(obj.Line);

		Controllable = true;
		CameraRotationControllable = true;
		InDialog = false;

		if (IsInstanceValid(tween) && tween.IsRunning()) tween.Stop();

		SceneTreeTimer timer = GetTree().CreateTimer(0.3f);
		await ToSignal(timer, Timer.SignalName.Timeout);
		timer.Dispose();

		obj.Cooldown = false;
	}


	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured) {
			var mouse = @event as InputEventMouseMotion;
			LookDirection = mouse.Relative * 0.001f;

			if (MouseCaptured) RotateCamera();
		}

		// if (!controllable) return;
		
		if (Controllable && Walking && !Sprinting && !Crouching && Input.IsActionPressed("Sprint") && Input.IsActionPressed("MoveForward")) {
			Sprinting = true;
		}
		
		else if (Sprinting && (!Input.IsActionPressed("Sprint") || !Input.IsActionPressed("MoveForward") || !Walking)) {
			Sprinting = false;
		}

		if (Controllable && !Sprinting && !Crouching && IsOnFloor() && Input.IsActionPressed("Crouch")) {
			Crouching = true;
		}
		else if (Controllable && Crouching && !Input.IsActionPressed("Crouch")) {
			Crouching = false;
		}

		if (Controllable && Input.IsActionPressed("Jump") && !Crouching) {
			Jumping = true;
		}
		// if (Input.IsActionJustPressed("Exit")) GetTree().Quit();
	}

	public void CaptureMouse()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		MouseCaptured = true;
	}

	public void ReleaseMouse()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
		MouseCaptured = false;
	}

	private void RotateCamera(float sens_mod = 1.0f)
	{
		if (!CameraRotationControllable) return;
		Camera.RotateY( -(LookDirection.X * CameraSensitivity * sens_mod) );

		var x = Mathf.Clamp(Camera.Rotation.X - LookDirection.Y * CameraSensitivity * sens_mod, -1.5f, 1.5f);

		Camera.Rotation = new Vector3(x, Camera.Rotation.Y, Camera.Rotation.Z);
	}

	private void JoypadControls(float delta, float sens_mod = 10)
	{
		Vector2 joypad_dir = Input.GetVector("LookLeft", "LookRight", "LookUp", "LookDown");
		if (joypad_dir.Length() > 0) {
			LookDirection += joypad_dir * delta;
			
			RotateCamera(sens_mod);
			
			LookDirection = Vector2.Zero;
		}
	}

	private Vector3 GetMove(float delta)
	{
		if (!Controllable) return Vector3.Zero;
		MoveDirection = Input.GetVector("MoveLeft", "MoveRight", "MoveForward", "MoveBack");

		Vector3 _forward = Camera.GlobalTransform.Basis * new Vector3(MoveDirection.X, 0, MoveDirection.Y);
		Vector3 walk_dir = new Vector3(_forward.X, 0, _forward.Z).Normalized();

		Walking = !walk_dir.IsZeroApprox();
		
		WalkVelocity = WalkVelocity.MoveToward(walk_dir * WalkSpeed * MoveDirection.Length(), Acceleration * delta);
		return WalkVelocity;
	}

	private Vector3 GetGravity(float delta)
	{
		GravityVelocity = IsOnFloor() ? Vector3.Zero : GravityVelocity.MoveToward(new Vector3(0, Velocity.Y - Game.Instance.Gravity, 0), Game.Instance.Gravity * delta);
		return GravityVelocity;
	}

	private Vector3 GetJump(float delta)
	{
		if (Jumping) {
			JumpVelocity = IsOnFloor() ? new Vector3(0, Mathf.Sqrt(4 * JumpHeight * Game.Instance.Gravity), 0) : JumpVelocity;
			Jumping = false;

			return JumpVelocity;
		}

		JumpVelocity = (!Controllable || IsOnFloor()) ? Vector3.Zero : JumpVelocity.MoveToward(Vector3.Zero, Game.Instance.Gravity * delta);
		return JumpVelocity;
	}

	private void SpeedEffect(float zoommod, float speed, float offset, bool collision2, float _t, double delta)
	{
		WalkSpeed = this.Twlerp(WalkSpeed, speed, _t, delta);
		Camera.Fov = this.Twlerp(Camera.Fov, BaseFov+zoommod, _t, delta);
		CameraOffset = this.Twlerp(CameraOffset, offset, _t, delta);

		Collision.Disabled = collision2;
		Collision2.Disabled = !collision2;
	}

	private void TiltEffect(float degrees, float _t, double delta)
	{
		float z = this.Twlerp(Camera.RotationDegrees.Z, degrees, _t, delta);
		Camera.RotationDegrees = new Vector3(Camera.RotationDegrees.X, Camera.RotationDegrees.Y, z);
	}


	/// <summary>
	/// idk what to name it but this variable is responsible for the different walk effect stages
	/// </summary>
	private bool WalkerGuh = false;

	public async Task WalkEffect(double delta)
	{
		/*if (WalkingEffecting) return;

		WalkingEffecting = true;

		Transform3D Global = Camera.GlobalTransform;
		Vector3 Origin = Global.Origin;

		Vector3 ForwardVector = Global.Basis.Z;
		Vector3 LeftVector = Global.Basis.X;
		Vector3 UpVector = Global.Basis.Y;

		float X = (WalkerGuh ? -WalkSpeed : WalkSpeed)/10;
		// float Y = WalkerGuh ? -5 : -WalkSpeed;

		WalkerGuh = !WalkerGuh;

		GD.Print($"X: {X}");
		// GD.Print($"Y: {Y}");

		Origin += LeftVector * X;
		// Origin += UpVector * Y;

		Transform3D Transform = new Transform3D(LeftVector, UpVector, ForwardVector, Origin);

		Camera.GlobalTransform = Camera.GlobalTransform.InterpolateWith(Transform, this.FactorDelta(1/1.5f, delta));

		await Task.Delay(200);

		WalkingEffecting = false;*/
	}
}
