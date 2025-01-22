using Godot;
using Godot.Collections;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Runtime.InteropServices;
using System.Linq;
using System.Threading.Tasks;


#region Shaker3D


[GlobalClass, Icon("res://addons/shylib/Images/Shaker3D.png")]
public partial class Shaker3D : Node
{

	[Export] public Node3D Instance;
	[Export] public Shaker3DPreset Preset = Shaker3DPreset.None;

	[Export] public Shaker3DInstance ShakeInstance = new();
	[Export] public bool AutoSustain = false;

	[Export] public bool RunOnStart = false;
	public bool Running = false;
	

	private Vector3 PositionAddShake = Vector3.Zero;
	public Vector3 RotationAddShake = Vector3.Zero;


	public override void _Ready() {
		Instance = Instance == null ? GetParent<Node3D>() : Instance;
		Running = RunOnStart;

		if (Preset == Shaker3DPreset.DONOTUSETHISTHISISBADLEAVEITALONE) throw new NotImplementedException("Shaker3DPreset should not be Unknown (Preset is likely null)");
		else {
			if (Preset != Shaker3DPreset.None) ShakeInstance = PresetsData[Preset.ToString()];
		}

		ShakeInstance.Shaker3D = this;
	}

	public async override void _Process(double delta)
	{
		if (Running) {
			Transform3D Updated = await Update(delta);
			Instance.GlobalTransform *= Updated;
		}
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

		Transform3D Transform = new(Basis.FromEuler(new Vector3(
				Mathf.DegToRad(RotationAddShake.X), 
				Mathf.DegToRad(RotationAddShake.Y), 
				Mathf.DegToRad(RotationAddShake.Z)
		), EulerOrder.Yxz), PositionAddShake);
	
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

	public Shaker3DInstance Shake(Shaker3DInstance instance) {
		ShakeInstance = instance;
		return instance;
	}

	public Shaker3DInstance ShakeSustain(Shaker3DInstance instance) {
		ShakeInstance = instance;
		instance.StartFadeIn(ShakeInstance.FadeInDuration);
		return instance;
	}

	public Shaker3DInstance ShakeOnce(float magnitude, float roughness, float fadeInTime, float fadeOutTime, Vector3 posInfluence = new(), Vector3 rotInfluence = new()) {
		Shaker3DInstance instance = new() {
			Magnitude = magnitude,
			Roughness = roughness,
			FadeInDuration = fadeInTime,
			FadeOutDuration = fadeOutTime,
			PositionInfluence = posInfluence,
			RotationInfluence = rotInfluence
		};

		instance.StartFadeIn(fadeInTime);

		return instance;
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
		None = 1,
		Bump = 2,
		Explosion = 3,
		Earthquake = 4,
		BadTrip = 5,
		HandheldCamera = 6,
		Vibration = 7,
		RoughDriving = 8
	};

	public static Dictionary<string, Shaker3DInstance> PresetsData { get; set; } = new() {
		{"Bump", new Shaker3DInstance {
			Magnitude = 2.5f,
			Roughness = 4,
			FadeInDuration = 0.1f,
			FadeOutDuration = 0.75f,
			PositionInfluence = new Vector3(0.15f, 0.15f, 0.15f),
			RotationInfluence = new Vector3(1, 1, 1)
		}},

		{"Explosion", new Shaker3DInstance {
			Magnitude = 5,
			Roughness = 10,
			FadeInDuration = 0,
			FadeOutDuration = 1.5f,
			PositionInfluence = new Vector3(0.25f, 0.25f, 0.25f),
			RotationInfluence = new Vector3(4, 1, 1)
		}},

		{"Earthquake", new Shaker3DInstance {
			Magnitude = 0.6f,
			Roughness = 3.5f,
			FadeInDuration = 2,
			FadeOutDuration = 10,
			PositionInfluence = new Vector3(0.25f, 0.25f, 0.25f),
			RotationInfluence = new Vector3(1, 1, 4)
		}},

		{"BadTrip", new Shaker3DInstance {
			Magnitude = 10,
			Roughness = 0.15f,
			FadeInDuration = 5,
			FadeOutDuration = 10,
			PositionInfluence = new Vector3(0, 0, 0.15f),
			RotationInfluence = new Vector3(2, 1, 4)
		}},

		{"HandheldCamera", new Shaker3DInstance {
			Magnitude = 1,
			Roughness = 0.25f,
			FadeInDuration = 5,
			FadeOutDuration = 10,
			PositionInfluence = new Vector3(0, 0, 0),
			RotationInfluence = new Vector3(1, 0.5f, 0.5f)
		}},

		{"Vibration", new Shaker3DInstance {
			Magnitude = 0.4f,
			Roughness = 20,
			FadeInDuration = 2,
			FadeOutDuration = 2,
			PositionInfluence = new Vector3(0, 0.15f, 0),
			RotationInfluence = new Vector3(1.25f, 0, 4)
		}},

		{"RoughDriving", new Shaker3DInstance {
			Magnitude = 1,
			Roughness = 2,
			FadeInDuration = 1,
			FadeOutDuration = 1,
			PositionInfluence = new Vector3(0, 0, 0),
			RotationInfluence = new Vector3(1, 1, 1)
		}}
	};
}


#endregion
#region Shaker3DInstance


[GlobalClass, Icon("res://addons/shylib/Images/Shaker3D.png")]
public partial class Shaker3DInstance : Resource
{
	[Export] public float Magnitude = 0;
	[Export] public float Roughness = 0;

	[Export] public Vector3 PositionInfluence = Vector3.Zero;
	[Export] public Vector3 RotationInfluence = Vector3.Zero;
	[Export] public bool StopOnInactive = true;

	[Export] public float RoughnessMod = 1;
	[Export] public float MagnitudeMod = 1;
	[Export] public float FadeInDuration = 0;
	[Export] public float FadeOutDuration = 0;

	private bool _sustain;

	[Export] public bool Sustain {
		get {
			if (Shaker3D != null && Shaker3D.AutoSustain) return true;
			else return _sustain;
		}

		set {
			_sustain = value;
		}
	}
	
	public Shaker3D Shaker3D;

	private float CurrentFadeTime;

	public bool Ready = false;

	public Shaker3D.ShakeState State {
		get {
			if (IsFadingIn()) return Shaker3D.ShakeState.FadingIn;
			else if (IsFadingOut()) return Shaker3D.ShakeState.FadingOut;
			else if (IsShaking()) return Shaker3D.ShakeState.Sustained;
			return Shaker3D.ShakeState.Inactive;
		}
	}

	private static readonly FastNoiseLite noise = new();
	private int Tick = new RandomNumberGenerator().RandiRange(-100, 100);


	public Shaker3DInstance() : base() 
	{
		CurrentFadeTime = (FadeInDuration > 0) ? 0 : 1;
		Sustain = Sustain ? Sustain : FadeInDuration > 0;
		Ready = true;
	}


	public Vector3 UpdateShake(double delta)
	{
		float _tick = Tick;
		float cft = CurrentFadeTime;

		Vector3 Offset = new Vector3(
			noise.GetNoise2D(_tick, 0) * 0.5f,
			noise.GetNoise2D(0, _tick) * 0.5f,
			noise.GetNoise2D(_tick, _tick) * 0.5f
		);

		if (FadeInDuration > 0 && Sustain) {
			if (cft < 1) cft += (float)delta / FadeInDuration;
			else if (FadeOutDuration > 0) Sustain = false;
		}

		if (Sustain) {
			Tick = (int)(_tick + (float)(delta * Roughness * RoughnessMod));
		}
		else {
			cft -= (float)delta / FadeOutDuration;

			Tick = (int)(_tick + (float)(delta * Roughness * RoughnessMod * CurrentFadeTime));
		}

		CurrentFadeTime = cft;

		return Offset * Magnitude * MagnitudeMod * cft;
	}

	public void StartFadeOut(object FadeOutTime = null)
	{
		if (FadeOutTime == null) CurrentFadeTime = 0;

		FadeOutDuration = (FadeOutTime == null) ? FadeOutDuration : (float)FadeOutTime;
		FadeInDuration = 0;
		Sustain = false;
	}


	public void StartFadeIn(object FadeInTime = null)
	{
		if (FadeInTime == null) CurrentFadeTime = 1;

		FadeInDuration = (FadeInTime == null) ? FadeInDuration : (float)FadeInTime;
		FadeOutDuration = 0;
		Sustain = true;
	}


	public bool IsShaking()
	{
		return CurrentFadeTime > 0 || Sustain;
	}

	public bool IsFadingOut()
	{
		return (!Sustain) && CurrentFadeTime > 0;
	}

	public bool IsFadingIn()
	{
		return CurrentFadeTime < 1 && Sustain && FadeInDuration > 0;
	}
}

#endregion
