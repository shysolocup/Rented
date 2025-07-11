using Godot;
using CoolGame;

[GlobalClass, Icon("uid://ctfx6n3wqvqla")]
public partial class Crosshair3D : StaticBody3D
{
	[Export] public StandardMaterial3D DefaultIcon;
	[Export] public StandardMaterial3D DefaultHoverIcon;
	[Export] public StandardMaterial3D LockedIcon;
	[Export] public float BaseDistance = 30;
	private Vector3 BaseScale;
	[Export] public Camera3D Camera;
	[Export] public Player Player;
	[Export] public string Delay = "1/1.7";
	
	public float Distance = 0;

	public MeshInstance3D Icon;
	public Camera3D ViewportCamera;

	private SubViewportContainer CrosshairContainer;

	private float _delay = 0;

	private float ShakeX;
	private float ShakeY;

	public override void _Ready()
	{
		Distance = BaseDistance;
		Delay ??= "1/1";

		string[] spl = Delay.Split("/");
		_delay = float.Parse(spl[0]) / float.Parse(spl[1]);

		Camera ??= this.GetGameNode<Camera3D>("%PlayerCamera");
		Player = this.GetGameNode<Player>("%Player");
		Icon = GetNode<MeshInstance3D>("./Icon");
		ViewportCamera = GetNodeOrNull<Camera3D>("../ViewportCamera");
		CrosshairContainer = GetParent().GetParent<SubViewportContainer>();

		BaseScale = Icon.Scale;
		Icon.SetSurfaceOverrideMaterial(0, DefaultIcon);
	}

	public override void _Process(double delta)
	{
		if (Camera is not null)
		{
			Transform3D Global = Camera.GlobalTransform;
			Vector3 Origin = Global.Origin;

			Vector3 ForwardVector = Global.Basis.Z;
			Vector3 LeftVector = Global.Basis.X;
			Vector3 UpVector = Global.Basis.Y;

			double Noise = Mathf.Clamp(Game.Instance.Noise / 400, 0, 1);

			float ShakeX = (float)GD.RandRange(-Noise, Noise) * 1.2f;
			float ShakeY = (float)GD.RandRange(-Noise, Noise) * 1.2f;

			float NoiseMod = 1 - (float)Mathf.Clamp(Game.Instance.Noise / 100, 0, 1);

			float A = CrosshairContainer.Modulate.R;

			// A = A + (B - A) * t

			float NoiseDelay = this.FactorDelta(1 / 5f, delta);

			float Alpha = 1.25f - (float)Mathf.Clamp(A + (NoiseMod - A) * NoiseDelay, 0, 1); // 10 noise is 0.1 alpha

			float GB = A + (NoiseMod - A) * 1 / 3f; // 10 noise is 0.9 green and blue

			Distance += (BaseDistance - Mathf.Clamp(Game.Instance.Noise * 0.1f + 5, 1, 20) - Distance) * NoiseDelay; // 10 noise is 6 meters distance

			/*
				should always be 255 for red
				as noise rises to 100 it'll turn the crosshair red and make it shake rapdily
				as noise rises it'll also make the crosshair more visible increasing it's size and alpha
			*/
			CrosshairContainer.Modulate = new Color(1, GB, GB, Alpha);

			Origin += ForwardVector * -Distance;
			Origin += LeftVector * ShakeX;
			Origin += UpVector * ShakeY;

			Transform3D IconTransform = new(LeftVector, UpVector, ForwardVector, Origin);

			Icon.GlobalTransform = Icon.GlobalTransform.InterpolateWith(IconTransform, this.FactorDelta(_delay, delta));

			if (ViewportCamera != null) ViewportCamera.GlobalTransform = Global;
		}
	}
}
