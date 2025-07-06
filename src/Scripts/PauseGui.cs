using System;
using System.Linq;
using System.Threading.Tasks;
using CoolGame;
using Godot;

public partial class PauseGui : Control
{
	private bool paused = false;

	public Button ResumeButton;
	public Button SkipButton;
	public Button SettingsButton;
	public Button QuitButton;

	public VBoxContainer Container;

	static private Vector2 GradientStartFrom = new(0, 0);
	static private Vector2 GradientEndFrom = new(10, 0);

	private TextureRect Background;
	private SettingsGui Settings;

	private TextureRect GradientTexture;
	private GradientTexture2D Gradient;
	private bool cooldown = false;

	private Player Player;
	private Lighting3D Lighting;

	private string ImageDir = "res://src/Textures/PauseImages";

	public override void _Ready()
	{
		Player = this.GetGameNode<Player>("%Player");
		Lighting = this.GetGameNode<Lighting3D>("%Lighting3D");
		Settings = this.GetGameNode<SettingsGui>("%SettingsLayer/SettingsGui");

		GradientTexture = GetNode<TextureRect>("Gradient");
		Gradient = GradientTexture.Texture as GradientTexture2D;
		GradientTexture.Hide();
		
		Background = GetNode<TextureRect>("Background");
		Background.Hide();

		Container = GetChild<VBoxContainer>(0);

		var ButtonContainer = Container.GetChild(2);

		ResumeButton = ButtonContainer.GetChild<Button>(0);
		SkipButton = ButtonContainer.GetChild<Button>(1);
		SettingsButton = ButtonContainer.GetChild<Button>(2);
		QuitButton = ButtonContainer.GetChild<Button>(3);

		ResumeButton.Pressed += Resume;
		SkipButton.Pressed += SkipCutscene;
		SettingsButton.Pressed += OpenSettings;
		QuitButton.Pressed += Quit;
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWMWindowFocusOut)
		{
			Paused = true;
		}
	}

	public bool Paused {
		get => paused;

		set {
			if (paused != value)
			{
				paused = value;

				GetTree().Paused = value;

				if (value)
				{
					/*defaultWorldEffects = World.Environment;
					defaultCameraEffects = World.CameraAttributes;

					World.Environment = weirdWorldEffects;
					World.CameraAttributes = weirdCameraEffects;
					World.CameraAttributes.AutoExposureScale = 1;*/
					Player.ReleaseMouse();
					Lighting.SetTempLighting("Pause");

					var dir = DirAccess.Open(ImageDir).GetFiles().Where(v => !v.Contains(".import")).ToArray();

					Gradient.FillFrom = GradientStartFrom;
					GradientTexture.Show();

					Background.Texture = GD.Load<Texture2D>($"{ImageDir}/{dir[GD.Randi() % dir.Length]}");
					Background.Show();
					// Show();
				}
				else
				{
					Lighting.TempLighting = null;
					GradientTexture.Hide();
					Lighting.ResetApply();
					Background.Hide();
				}

				if (!value && !Player.InDialog)
				{
					/*World.Environment = defaultWorldEffects;
					World.CameraAttributes = defaultCameraEffects;*/
					Player.CaptureMouse();
					// Hide();
				}

				SkipButton.Visible = Player.InDialog;
			}
		}
	}

	public void Resume()
	{
		Paused = false;
	}

	public void SkipCutscene()
	{
		Player.RunningDialogueToken.Cancel();
		Resume();
	}

	public void OpenSettings()
	{
		Settings.InSettings = true;
	}

	public void Quit()
	{
		GetTree().Quit();
	}

	private bool rebound = false;
	private bool waiting = false;

	public override async void _Process(double delta)
	{
		base._Process(delta);

		Modulate = Modulate.Lerp(paused ? Colors.White : Colors.Transparent, 1/0.5f*(float)delta);

		if (paused)
		{
			Container.Modulate = Container.Modulate.Lerp(!Settings.InSettings ? Colors.White : Colors.Transparent, 1/0.3f*(float)delta);
			Gradient.FillFrom = Gradient.FillFrom.Lerp(GradientEndFrom, 1 / 1 * (float)delta);
		}

		if (paused && !waiting && Lighting.TempLightingIs("Pause"))
		{
			var World = Lighting.World;
			float scale = World.CameraAttributes.AutoExposureScale;

			if (Math.Round(scale, 2) <= 0.3f && !rebound) rebound = true;
			if (Math.Round(scale) >= 1 && rebound) rebound = false;

			World.CameraAttributes.AutoExposureScale = this.Twlerp(scale, rebound ? 1 : 0.02f, 1 / 500f, delta);

			waiting = true;

			await Task.Delay(100);

			waiting = false;
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (Player != null && !DebugConsole.IsConsoleVisible() && Input.IsActionJustPressed("Pause"))
		{
			if (cooldown) return;

			cooldown = true;
			Game.Delay(0.1f, () => cooldown = false);

			if (Settings.InSettings)
			{
				Settings.InSettings = false;
				return;
			}

			Paused ^= true;
		}
	}
}
