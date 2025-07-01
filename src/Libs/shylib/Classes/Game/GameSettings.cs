using CoolGame;
using Godot;

[GlobalClass, Icon("uid://dwbnqewotd3wq")]
public partial class GameSettings : Node
{
	public enum ColorblindModes
	{
		None = 0,
		Protanopia = 1,
		Deutranopia = 2,
		Tritanopia = 3
	}

	[Export] public int TargetFPS = 60;
	[Export] public float ResolutionScale = 1;
	[Export] public float MinScale = .4f;
	[Export] public float MaxScale = 1;
	[Export] public float StepSize = .1f;
	[Export] public Label TextLabel;


	private float cbi = 0;
	private ColorblindModes cbm = ColorblindModes.None;

	[Export]
	public float ColorblindIntensity
	{
		get => cbi;
		set
		{
			cbi = value;
			UpdateColorblind();
		}
	}
	[Export] public ColorblindModes ColorblindMode
	{
		get => cbm;
		set
		{
			cbm = value;
			UpdateColorblind();
		}
	}
	
	private int MeasureFrames = 20;
	private int Measured = 0;
	private float Accumulate = 0;
	private int SkipFrames = 5;
	private int SkipAfterAdjustment = 5;
	private Rid ViewportRid;
	private Viewport Viewport;
	private Player Player;
	private ShaderMaterial Colorblind;


	public void UpdateColorblind()
	{
		GD.Print((int)ColorblindMode-1);
		GD.Print(ColorblindIntensity);
		Colorblind.SetShaderParameter("mode", (int)ColorblindMode-1);
		Colorblind.SetShaderParameter("intensity", ColorblindMode == ColorblindModes.None ? 0 : ColorblindIntensity);
	}


	public override void _Ready()
	{
		base._Ready();
		Viewport = GetViewport();
		ViewportRid = Viewport.GetViewportRid();
		RenderingServer.ViewportSetMeasureRenderTime(ViewportRid, true);
		Viewport.Scaling3DScale = ResolutionScale;

		Player = this.GetGameNode<Player>("%Player");
		Colorblind = GetChild(0).GetChild<ColorRect>(0).Material as ShaderMaterial;

		Game.Settings = this;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (SkipFrames > 0)
		{
			SkipFrames -= 1;
			return;
		}

		if (Measured < MeasureFrames) {
			Measured += 1;

			double FrameTimeCPU = RenderingServer.ViewportGetMeasuredRenderTimeCpu(ViewportRid) + RenderingServer.GetFrameSetupTimeCpu();
			double FrameTimeGPU = RenderingServer.ViewportGetMeasuredRenderTimeGpu(ViewportRid);
			double FrameTime = (FrameTimeCPU + FrameTimeGPU)/1000;

			Accumulate += (float)FrameTime;
		}

		else {
			float TargetFrameTime = 1/TargetFPS;
			float CurrentFrameTime = Accumulate/Measured;
			float Relative = CurrentFrameTime/TargetFrameTime;
			float RelResolution = Mathf.Pow(Relative, .5f);

			float NewScale = ResolutionScale/RelResolution;
			NewScale = Mathf.Floor(NewScale/StepSize)*StepSize;
			NewScale = Mathf.Clamp(NewScale, MinScale, MaxScale);

			int CurrentFPS = Mathf.RoundToInt(1/CurrentFrameTime);

			if (TextLabel != null) {
				TextLabel.Text = $"{CurrentFPS}fps at {Mathf.RoundToInt(ResolutionScale*100)}%";
			}

			if (NewScale != ResolutionScale) {
				GD.Print($"going from scale {ResolutionScale} to {NewScale} at {CurrentFPS}fps");
				Viewport.Scaling3DScale = NewScale;
				ResolutionScale = NewScale;
				SkipFrames = SkipAfterAdjustment;
			}
		}

		Measured = 0;
		Accumulate = 0;
	}
}
