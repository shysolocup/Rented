using Godot;
using Godot.Collections;
using System;
using CoolGame;
using System.Threading.Tasks;
using System.Threading;

[Tool]
[GlobalClass]
public partial class Player : CharacterBody3D
{

	[ExportCategory("Player")]

	static public Inventory Inventory;


	#region Dynamic Vars
	[ExportGroup("Dynamics")]

	[Export(PropertyHint.Range, "1,35,1")] public float Speed = 10; // m/s
	[Export(PropertyHint.Range, "1,35,1")] public float BaseWalkSpeed = 3; // m/s
	[Export(PropertyHint.Range, "1,35,1")] public float WalkSpeed = 3; // m/s
	[Export(PropertyHint.Range, "10,400,1")] public float Acceleration = 100; // m/s^2
	[Export(PropertyHint.Range, "0.1,3.0,0.1")] public float JumpHeight = 0.3f; // m

	[Export] public bool InDanger = false;

	private Vector2 MoveDirection; // Input direction for movement
	private Vector2 LookDirection; // Input direction for look/aim
	private Vector3 WalkVelocity; // Walking velocity 
	private Vector3 GravityVelocity; // Gravity velocity 
	private Vector3 JumpVelocity; // Jumping velocity
	#endregion


	#region Camera Vars
	[ExportGroup("Camera")]

	[ExportToolButton("Reset Camera")] private Callable ResetCameraCall => Callable.From(ResetCamera);

	private void ResetCamera() {
		Camera = GetNode<Camera3D>("%PlayerCamera");

		Camera.GlobalPosition = new Vector3(GlobalPosition.X, GlobalPosition.Y + DefaultCameraOffset, GlobalPosition.Z);
		Camera.GlobalRotation = GlobalRotation;
	}

	[Export] public float BaseFov;

	[Export] public float TiltRotation = 3;

	private Camera3D Camera;

	[Export(PropertyHint.Range, "0.1,3.0,0.1,or_greater")] public float CameraSensitivity = 0.5f;

	[Export] public float BobbleFrequency = 0.6f;
	[Export] public float BobbleAmplifier = 0.1f;
	static public float BobbleTime = 0;

	[Export] public bool CameraRotationControllable = true;
	[Export] public bool CameraPositionControllable = true;
	[Export] public bool Tilt = true;

	private float DefaultCameraOffset = 1f;
	private float CameraOffset;

	#endregion


	#region Sprint Vars
	[ExportGroup("Sprinting")]
	
	[Export] public float SprintSpeedMod = 2; // m/s
	[Export] public float SprintFovMod = 4;
	[Export] public bool Sprinting = false;
	[Export] public bool CanSprint = true;

	#endregion


	#region Crouch Vars
	[ExportGroup("Crouching")]
	
	[Export] public float CrouchSpeedMod = -2; // m/s
	[Export] public float CrouchFovMod = -20;
	[Export] public bool Crouching = false;
	[Export] public bool CanCrouch = true;

	#endregion


	#region Zoom Vars
	[ExportGroup("Zooming")]
	[Export] public float ZoomedSpeedMod = -0.5f; // m/s
	[Export] public float ZoomFovMod = -40;
	[Export] public bool Zoomed = false;
	[Export] public bool CanZoom = true;


	#endregion

	#region Switch Vars
	[ExportGroup("Switches")]

	[Export] public bool TabbedIn = true;
	[Export] public bool Controllable = true;
	[Export] public bool Jumping = false;
	[Export] public bool InDialog = false; 
	[Export] public bool MouseCaptured = false;
	[Export] public bool ActionCooldown = false;
	[Export] public bool Dead = false;
	[Export] public bool Walking = false;
	private bool WalkingEffecting = false;

	#endregion


	#region Freecam Vars	
	[ExportGroup("Freecam")]

	[Export] public bool Freecam = false;

	private Transform3D FreecamOrigin = Transform3D.Identity;
	private Vector2 FreecamMousePosition = Vector2.Zero;
	private float FreecamTotalPitch = 0;
	private Vector3 FreecamDirection = Vector3.Zero;
	private Vector3 FreecamVelocity = Vector3.Zero;
	private int FreecamAcceleration = 100;
	private float FreecamVelMultiplier = 4;
	private float FreecamSpeed = 1.3f;
	private float FreecamShiftMult = 2f;
	#endregion


	#region Interact Vars
	[ExportGroup("Interactions")]

	[Export] public float InteractFovMod = -15;
	
	private Transform3D? CameraSubject;
	
	public RayCast3D Raycast;
	public InteractObject3D Inter;
	#endregion


	private Variant nullvar = new();
	private CollisionShape3D Collision;
	private CollisionShape3D Collision2;
	private MeshInstance3D Mesh;

	private Vector3 Bobble = Vector3.Zero;

	static public Task RunningDialogue;
	static public CancellationTokenSource RunningDialogueToken;

	[Signal] public delegate void StartDialogueEventHandler(Array<DialogueSequence> scene);

	[Signal] public delegate void FinishDialogueEventHandler(Array<DialogueSequence> scene);

	#region Die
	public void Die(int id = 0) 
	{
		
	}
	#endregion
	
	#region Ready
	public override void _Ready()
	{	
		Inventory = this.GetGameNode<Inventory>("%Inventory");
		Camera = GetNode<Camera3D>("%PlayerCamera");
		Raycast = Camera.GetChild<RayCast3D>(0);

		Collision = GetNode<CollisionShape3D>("./Collision");
		Collision2 = GetNode<CollisionShape3D>("./CrouchCollision");
		Mesh = GetNode<MeshInstance3D>("./Mesh");

		CameraOffset = DefaultCameraOffset;

		BaseFov = Camera.GetFov();
		if (!Engine.IsEditorHint()) CaptureMouse();

		GD.Print(Get("camera"));

		// CrouchEffect("crouchIn", crouchFovMod, crouch_speed, 0.5f);
		// CrouchEffect("crouchOut", 0, base_walk_speed, 1);

		// console = GetTree().
	}
	#endregion 

	#region Notification
	public override void _Notification(int what)
	{
		if (Engine.IsEditorHint()) return;
		if (what == NotificationWMWindowFocusIn) {
			TabbedIn = true;
		}
		else if (what == NotificationWMWindowFocusOut) {
			TabbedIn = false;

			Crouching = false;
			Sprinting = false;
		}
	}
	#endregion

	#region PhysicsProcess
	public override void _PhysicsProcess(double delta)
	{
		if (Engine.IsEditorHint()) return;
		// if (!controllable) return;
		if (Freecam) return;
		
		float d = (float)delta;
		if (MouseCaptured) JoypadControls(d);

		Velocity = GetMove(d) + GetGravity(d) + GetJump(d);
		MoveAndSlide();
	}
	#endregion

	#region UpdateFreecam
	public void UpdateFreecamMovement(double delta)
	{
		Vector2 V2Direction = Input.GetVector("MoveLeft", "MoveRight", "MoveForward", "MoveBack");

		Vector3 _forward = Camera.GlobalTransform.Basis * new Vector3(V2Direction.X, 0, V2Direction.Y);
		FreecamDirection = new Vector3(_forward.X, Convert.ToInt32(Input.IsKeyPressed(Key.E)) - Convert.ToInt32(Input.IsKeyPressed(Key.Q)), _forward.Z).Normalized();

		float Speed = FreecamSpeed;

		if (Input.IsKeyPressed(Key.Shift)) Speed *= FreecamShiftMult;
		if (Input.IsKeyPressed(Key.Ctrl)) Speed *= 1/FreecamShiftMult;
		
		FreecamVelocity = FreecamVelocity.MoveToward(FreecamDirection * Speed * V2Direction.Length(), FreecamAcceleration * (float)delta);
		
		GD.Print(FreecamVelocity);
		
		Camera.Translate(FreecamVelocity * (float)delta * Speed);
	}
	#endregion

	#region Process
	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint()) return;


		// Controls

		// sprint
		if (CanSprint && Controllable && Walking && !Zoomed && !Sprinting && !Crouching && Input.IsActionPressed("Sprint") && Input.IsActionPressed("MoveForward")) Sprinting = true;
		else if (Sprinting && (!Input.IsActionPressed("Sprint") || !Input.IsActionPressed("MoveForward") || !Walking)) Sprinting = false;

		// crouch
		if (CanCrouch && Controllable && !Sprinting && !Crouching && IsOnFloor() && Input.IsActionPressed("Crouch")) Crouching = true;
		else if (Controllable && Crouching && !Input.IsActionPressed("Crouch")) Crouching = false;

		// zoom
		if (CanZoom && Controllable && !Zoomed && Input.IsActionPressed("Zoom")) Zoomed = true;
		else if (!Input.IsActionPressed("Zoom")) Zoomed = false;

		// jump
		if (Controllable && Input.IsActionPressed("Jump") && !Crouching) Jumping = true;

		
		if (Freecam)
		{
			UpdateFreecamMovement(delta);
		}

		if (Raycast.IsColliding()) {
			GodotObject result = Raycast.GetCollider();

			if (Inter != null && result != Inter) {
				Inter.Hovering = false;
				Inter = null;
			}

			if (result is InteractObject3D collider && GlobalPosition.DistanceTo(collider.GlobalPosition) <= collider.Distance) {
				Inter = collider;
				collider.Hovering = true;
			}
		}

		else if (Inter != null) {
			Inter.Hovering = false;
			Inter = null;
		}

		if (Camera != null) {

			BobbleTime = (Velocity.Length() > 0 && BobbleTime < 100) ? BobbleTime + (float)(delta * Velocity.Length()) * 3 : 0;

			if (CameraPositionControllable && !Freecam) {
				Bobble = this.Twlerp(Bobble, HeadBobble(), 1/15f, delta);
				Camera.Position = Camera.Position.Lerp(new Vector3(Position.X, Position.Y + CameraOffset, Position.Z) + Bobble + (JumpVelocity/40), 17 * (float)delta);
			}
			if (!CameraRotationControllable && CameraSubject is Transform3D origin) {
				SnatchCamera(origin, delta);
			}
			if (!Freecam) Rotation = new Vector3(Rotation.X, Camera.Rotation.Y, Rotation.Z);


			var fov = InDialog ? InteractFovMod : 0;
			float speed = BaseWalkSpeed;
			var height = DefaultCameraOffset;
			float factor = 1/30f;

			if (Crouching)
			{
				fov += CrouchFovMod;
				speed += CrouchSpeedMod;
				height = -0.4f;
				factor = 1 / 25f;
			}

			if (Sprinting)
			{
				fov += SprintFovMod;
				speed += SprintSpeedMod;
				factor = 1 / 45f;
			}

			if (Zoomed)
			{
				fov += ZoomFovMod;
				speed += ZoomedSpeedMod;
				factor = 1 / 25f;
			}


			InputEffect( fov, speed, height, Crouching, factor, delta);



			// tilt effect
			if (Controllable && Input.IsActionPressed("MoveLeft"))
			{
				TiltEffect(TiltRotation, 1 / 12f, delta);
			}
			else if (Controllable && Input.IsActionPressed("MoveRight"))
			{
				TiltEffect(-TiltRotation, 1 / 12f, delta);
			}
			else if (Camera != null && Camera.RotationDegrees.Z != 0)
			{
				TiltEffect(0, 1 / 12f, delta);
			}
		}
	}
	#endregion


	#region PlayDialogue
	public async Task PlayDialogue(string line)
	{
		Controllable = false;
		CameraRotationControllable = false;
		InDialog = true;
		ReleaseMouse();
		
		DialogueData data = this.GetGameNode("Guis/DialogueGui").GetNode<DialogueData>("%DialogueData");

		GD.Print(data);

		using var tokenSource = new CancellationTokenSource();
		RunningDialogueToken = tokenSource;
		
		RunningDialogue = data.Play(line, tokenSource.Token);
		await RunningDialogue.WaitAsync(tokenSource.Token);

		RunningDialogue = null;
		tokenSource.Dispose();

		Controllable = true;
		CameraRotationControllable = true;
		InDialog = false;
		CaptureMouse();
	}
	#endregion

	#region SnatchCamera
	public void SnatchCamera(Transform3D origin, double delta)
	{
		Vector3 Direction = (origin.Origin - Camera.GlobalTransform.Origin).Normalized();
		Basis TargetBasis = Basis.LookingAt(Direction, Vector3.Up);
		Camera.GlobalRotation = Camera.GlobalRotation.Lerp(TargetBasis.GetEuler(), this.FactorDelta(1/10f, delta));
	}
	#endregion

	#region SnatchInteract
	public async Task SnatchInteract(TalkerObject3D obj) 
	{
		GD.Print(obj.Cooldown);

		if (obj.Cooldown) {
			obj.Cooldown = false; return;
		}

		obj.Cooldown = true;

		CameraSubject = obj.GlobalTransform;

		await PlayDialogue(obj.Line);

		CameraSubject = null;

		await GetTree().CreateTimer(0.5f).Guh();

		obj.Cooldown = false;
	}
	#endregion

	#region UnhandledInput
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouse && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			if (Freecam) FreecamMousePosition = mouse.Relative;
			LookDirection = mouse.Relative * 0.001f;

			if (MouseCaptured) RotateCamera();
		}

		if (Freecam && @event is InputEventMouseButton button && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			switch (button.ButtonIndex)
			{
				case MouseButton.Right:
					Input.SetMouseMode( button.Pressed ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible);
					break;

				case MouseButton.WheelUp:
					FreecamVelMultiplier = Mathf.Clamp(FreecamVelMultiplier * 1.1f, 0.2f, 20);
					break;

				case MouseButton.WheelDown:
					FreecamVelMultiplier = Mathf.Clamp(FreecamVelMultiplier / 1.1f, 0.2f, 20);
					break;
			}
		}

		// if (!controllable) return;
		// if (Input.IsActionJustPressed("Exit")) GetTree().Quit();
	}
	#endregion

	#region CaptureMouse
	public void CaptureMouse()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		MouseCaptured = true;
	}
	#endregion

	#region ReleaseMouse
	public void ReleaseMouse()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
		MouseCaptured = false;
	}
	#endregion

	#region RotateCamera
	private void RotateCamera(float sens_mod = 1.0f)
	{
		if (!CameraRotationControllable) return;
		Camera.RotateY( -(LookDirection.X * CameraSensitivity * sens_mod) );

		var x = Mathf.Clamp(Camera.Rotation.X - LookDirection.Y * CameraSensitivity * sens_mod, -1.5f, 1.5f);

		Camera.Rotation = new Vector3(x, Camera.Rotation.Y, Camera.Rotation.Z);
	}
	#endregion

	#region JoypadControls
	private void JoypadControls(float delta, float sens_mod = 10)
	{
		Vector2 joypad_dir = Input.GetVector("LookLeft", "LookRight", "LookUp", "LookDown");
		if (joypad_dir.Length() > 0) {
			LookDirection += joypad_dir * delta;
			
			RotateCamera(sens_mod);
			
			LookDirection = Vector2.Zero;
		}
	}
	#endregion

	#region GetMove
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
	#endregion

	#region GetGravity
	private Vector3 GetGravity(float delta)
	{
		GravityVelocity = IsOnFloor() ? Vector3.Zero : GravityVelocity.MoveToward(new Vector3(0, Velocity.Y - Game.Instance.Gravity, 0), Game.Instance.Gravity * delta);
		return GravityVelocity;
	}
	#endregion

	#region GetJump
	private Vector3 GetJump(float delta)
	{
		if (Jumping) {
			JumpVelocity = IsOnFloor() 
				? new Vector3(0, Mathf.Sqrt(4 * JumpHeight * Game.Instance.Gravity), 0) 
				: JumpVelocity;

			Jumping = false;

			return JumpVelocity;
		}

		if (!Controllable || IsOnFloor()) JumpVelocity = Vector3.Zero;

		else if (IsOnCeiling() && JumpVelocity.Y > 0) {
			KinematicCollision3D guh = MoveAndCollide(JumpVelocity * delta);

			if (guh != null) {
				JumpVelocity = JumpVelocity.Bounce(guh.GetNormal()) / 5;
			}
		}
		
		else {
			JumpVelocity = JumpVelocity.MoveToward(Vector3.Zero, Game.Instance.Gravity * delta);
		}

		return JumpVelocity;
	}
	#endregion

	#region InputEffect
	private void InputEffect(float zoommod, float speed, float offset, bool collision2, float _t, double delta)
	{
		WalkSpeed = this.Twlerp(WalkSpeed, speed, _t, delta);
		Camera.Fov = this.Twlerp(Camera.Fov, BaseFov+zoommod, _t, delta);
		CameraOffset = this.Twlerp(CameraOffset, offset, _t, delta);

		Collision.Disabled = collision2;
		Collision2.Disabled = !collision2;
	}
	#endregion

	#region TiltEffect
	private void TiltEffect(float degrees, float _t, double delta)
	{
		float z = this.Twlerp(Camera.RotationDegrees.Z, degrees, _t, delta);
		Camera.RotationDegrees = new Vector3(Camera.RotationDegrees.X, Camera.RotationDegrees.Y, z);
	}
	#endregion

	#region HeadBobble
	public Vector3 HeadBobble()
	{
		Vector3 BobPos = Vector3.Zero;
		var Length = Mathf.Min(Velocity.Length(), 1);

		BobPos.Y = Mathf.Sin(BobbleTime * BobbleFrequency) * BobbleAmplifier * Length;
		BobPos.X = Mathf.Cos(BobbleTime * BobbleFrequency) * BobbleAmplifier * Length;
		BobPos.Z = Mathf.Cos(BobbleTime * BobbleFrequency) * BobbleAmplifier * Length * GD.RandRange(-1, 1);

		return BobPos;
	}
	#endregion
}
