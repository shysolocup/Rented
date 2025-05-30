using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;


[Tool]
[GlobalClass, Icon("res://src/Libs/shylib/Images/Shaker3D.png")]
public partial class Shaker3D : Node
{

	[Export] public Node3D Instance;

	private Shaker3DPreset _preset = Shaker3DPreset.Custom;

	[Export] public Shaker3DPreset Preset {
		get => _preset;
		set {
			if (value != Shaker3DPreset.Custom) {
				OverwriteInstanceFromPreset(value);
			}

			_preset = value;
		}
	}

	private Shaker3DInstance _s3dinstance = new();


	// when changed it'll set the Shaker3D property of the Shaker3DInstance to the parent
	[Export] public Shaker3DInstance ShakeInstance {
		get => _s3dinstance;

		set {
			if (value != _s3dinstance) {
				value.Shaker3D = this as Shaker3D;
				_s3dinstance = value;
			}
		}
	}


	[Export] public bool AutoSustain = false;
	[Export] public bool RunOnStart = false;
	
	private bool _running = false;


	[Signal] public delegate void FinishedEventHandler();
	[Signal] public delegate void StartedEventHandler();


	public bool Running {
		get => _running;
		set {
			if (value != _running) {
				_running = value;

				if (value) EmitSignal(SignalName.Started);
				else EmitSignal(SignalName.Finished);
			}
		}
	}


	public override void _Ready() {
		if (!Engine.IsEditorHint()) {
			Instance = Instance == null ? GetParent<Node3D>() : Instance;

			if (Preset == Shaker3DPreset.DONOTUSETHISTHISISBADLEAVEITALONE) throw new NotImplementedException("Shaker3DPreset should not be Unknown (Preset is likely null)");

			ShakeInstance.Shaker3D = this;
			Running = RunOnStart;
		}
	}

	public async override void _Process(double delta)
	{
		// if (!Engine.IsEditorHint()) GD.Print(Running);
		if (Running && !Engine.IsEditorHint()) {
			Transform3D Updated = await Update(delta);
			Instance.Transform *= Updated;
		}
	}

	public Shaker3DInstance OverwriteInstanceFromPreset(Shaker3DPreset preset)
	{
		var data = PresetsData[preset];

		foreach ( (string k, Variant v) in data ) {
			ShakeInstance.Set(k, v);
		}

		return ShakeInstance;
	}

	public async Task<Transform3D> Update(double delta)
	{
		Vector3 PositionAddShake = Vector3.Zero;
		Vector3 RotationAddShake = Vector3.Zero;
		
		if (ShakeInstance.State == ShakeState.Inactive && ShakeInstance.StopOnInactive) Running = false;
		
		else if (ShakeInstance.State != ShakeState.Inactive) {
			var waitTask = Task.Run(async () => {
				while (!ShakeInstance.Ready) await Task.Delay(25);
			});

			if (waitTask != await Task.WhenAny(waitTask, Task.Delay(5000))) throw new TimeoutException();

			Vector3 ShakeVect = ShakeInstance.UpdateShake(delta);

			PositionAddShake += ShakeVect * ShakeInstance.PositionInfluence;
			RotationAddShake += ShakeVect * ShakeInstance.RotationInfluence;
		}

		Basis ShakeBasis = Basis.FromEuler(new Vector3(Mathf.DegToRad(RotationAddShake.X), Mathf.DegToRad(RotationAddShake.Y), Mathf.DegToRad(RotationAddShake.Z)), EulerOrder.Yxz);
		
		Transform3D Transform = new(ShakeBasis, PositionAddShake);
	
		return Transform;
	}

	public void Start()
	{
		Running = true;
	}

	public void Stop()
	{
		Running = false;
	}

	public void StopSustained(object duration)
	{
		if (ShakeInstance.Sustain) ShakeInstance.StartFadeOut( duration == null ? ShakeInstance.FadeInDuration : duration);
	}

	public Shaker3DInstance Shake(Shaker3DInstance instance = null) {
		ShakeInstance = instance ?? ShakeInstance;
		return instance;
	}

	public Shaker3DInstance ShakeSustain(Shaker3DInstance instance = null) {
		ShakeInstance = instance ?? ShakeInstance;
		instance.StartFadeIn(ShakeInstance.FadeInDuration);
		return instance;
	}

	public Shaker3DInstance ShakeOnce(float magnitude, float roughness, float fadeInTime, float fadeOutTime, Vector3 posInfluence = new(), Vector3 rotInfluence = new()) {
		ShakeInstance = new() {
			Magnitude = magnitude,
			Roughness = roughness,
			FadeInDuration = fadeInTime,
			FadeOutDuration = fadeOutTime,
			PositionInfluence = posInfluence,
			RotationInfluence = rotInfluence
		};

		ShakeInstance.StartFadeIn(fadeInTime);

		return ShakeInstance;
	}

	public enum ShakeState {
		Unknown = 0,
		FadingIn = 1,
		FadingOut = 2,
		Sustained = 3,
		Inactive = 4
	}

	public enum Shaker3DPreset {
		DONOTUSETHISTHISISBADLEAVEITALONE = 0,
		Custom = 1,
		Bump = 2,
		Explosion = 3,
		Earthquake = 4,
		BadTrip = 5,
		HandheldCamera = 6,
		Vibration = 7,
		RoughDriving = 8
	};

	public static readonly Dictionary<Shaker3DPreset, Dictionary<string, Variant>> PresetsData = new() {
		
		[Shaker3DPreset.Bump] = new() {
			["Magnitude"] = 2.5f,
			["Roughness"] = 4,
			["FadeInDuration"] = 0.1f,
			["FadeOutDuration"] = 0.75f,
			["PositionInfluence"] = new Vector3(0.15f, 0.15f, 0.15f) * 0.28f,
			["RotationInfluence"] = new Vector3(1, 1, 1)
		},

		[Shaker3DPreset.Explosion] = new() {
			["Magnitude"] = 5,
			["Roughness"] = 10,
			["FadeInDuration"] = 0,
			["FadeOutDuration"] = 1.5f,
			["PositionInfluence"] = new Vector3(0.25f, 0.25f, 0.25f) * 0.28f,
			["RotationInfluence"] = new Vector3(4, 1, 1)
		},

		[Shaker3DPreset.Earthquake] = new() {
			["Magnitude"] = 0.6f,
			["Roughness"] = 3.5f,
			["FadeInDuration"] = 2,
			["FadeOutDuration"] = 10,
			["PositionInfluence"] = new Vector3(0.25f, 0.25f, 0.25f) * 0.28f,
			["RotationInfluence"] = new Vector3(1, 1, 4)
		},

		[Shaker3DPreset.BadTrip] = new() {
			["Magnitude"] = 10,
			["Roughness"] = 0.15f,
			["FadeInDuration"] = 5,
			["FadeOutDuration"] = 10,
			["PositionInfluence"] = new Vector3(0, 0, 0.15f) * 0.28f,
			["RotationInfluence"] = new Vector3(2, 1, 4)
		},

		[Shaker3DPreset.HandheldCamera] = new() {
			["Magnitude"] = 1,
			["Roughness"] = 0.25f,
			["FadeInDuration"] = 5,
			["FadeOutDuration"] = 10,
			["PositionInfluence"] = new Vector3(0, 0, 0) * 0.28f,
			["RotationInfluence"] = new Vector3(1, 0.5f, 0.5f)
		},

		[Shaker3DPreset.Vibration] = new() {
			["Magnitude"] = 0.4f,
			["Roughness"] = 20,
			["FadeInDuration"] = 2,
			["FadeOutDuration"] = 2,
			["PositionInfluence"] = new Vector3(0, 0.15f, 0) * 0.28f,
			["RotationInfluence"] = new Vector3(1.25f, 0, 4)
		},

		[Shaker3DPreset.RoughDriving] = new() {
			["Magnitude"] = 1,
			["Roughness"] = 2,
			["FadeInDuration"] = 1,
			["FadeOutDuration"] = 1,
			["PositionInfluence"] = new Vector3(0, 0, 0) * 0.28f,
			["RotationInfluence"] = new Vector3(1, 1, 1)
		}

	};
}
