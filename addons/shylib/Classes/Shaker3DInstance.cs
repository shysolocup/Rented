using Godot;

[Tool]
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

	private float Tick = new RandomNumberGenerator().RandfRange(-100, 100);
	private float CurrentFadeTime;
	
	public Shaker3D Shaker3D;

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

		if (!Sustain) {
			cft -= (float)delta / FadeOutDuration;

			Tick = _tick + ((float)delta * Roughness * RoughnessMod * CurrentFadeTime);
		}
		else {
			Tick = _tick + ((float)delta * Roughness * RoughnessMod);
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