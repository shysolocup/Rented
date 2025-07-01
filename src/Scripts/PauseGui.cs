using System;
using System.Linq;
using System.Threading.Tasks;
using Appox;
using Godot;

public partial class PauseGui : Control
{
	private bool paused = false;

	public Button ResumeButton;
	public Button SkipButton;
	public Button SettingsButton;
	public Button QuitButton;

	private TextureRect Background;

	private Player Player;
	private Lighting3D Lighting;

	private string ImageDir = "res://src/Textures/PauseImages";

	/*private WorldEnvironment World;
	private CameraAttributesPhysical weirdCameraEffects = GD.Load("res://src/Resources/Ui/PauseCameraAttributes.tres") as CameraAttributesPhysical;
	private Godot.Environment weirdWorldEffects = GD.Load("res://src/Resources/Ui/PauseEffects.tres") as Godot.Environment;
	private CameraAttributes defaultCameraEffects = GD.Load("res://src/Resources/Skies/CameraAttributesPractical.tres") as CameraAttributesPractical;
	private Godot.Environment defaultWorldEffects = GD.Load("res://src/Resources/Skies/Default.tres") as Godot.Environment;*/

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

					var dir = DirAccess.Open(ImageDir).GetFiles().Where( v => !v.EndsWith(".import") ).ToArray();

					Background.Texture = GD.Load<Texture2D>($"{ImageDir}/{dir[GD.Randi() % dir.Length]}");
					Background.Show();
					// Show();
				}
				else
				{
					Lighting.TempLighting = null;
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

	}

	public void Quit()
	{
		GetTree().Quit();
	}


	public override void _Ready()
	{
		Player = this.GetGameNode<Player>("%Player");
		Lighting = this.GetGameNode<Lighting3D>("%Lighting3D");
		
		Background = GetNode<TextureRect>("Background");
		Background.Hide();

		var Container = GetChild(0).GetChild(2);

		ResumeButton = Container.GetChild<Button>(0);
		SkipButton = Container.GetChild<Button>(1);
		SettingsButton = Container.GetChild<Button>(2);
		QuitButton = Container.GetChild<Button>(3);

		ResumeButton.Pressed += Resume;

		SkipButton.Pressed += SkipCutscene;
		
		SettingsButton.Pressed += OpenSettings;

		QuitButton.Pressed += Quit;

	}

	private bool rebound = false;
	private bool waiting = false;

	public override async void _Process(double delta)
	{
		base._Process(delta);

		Modulate = Modulate.Lerp(paused && !waiting ? Colors.White : Colors.Transparent, this.FactorDelta(1 / 15f, delta));

		if (paused && !waiting && Lighting.TempLightingIs("Night"))
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
		if (Player != null && !DebugConsole.IsConsoleVisible() && @event is InputEventKey eventKey && eventKey.Pressed && eventKey.Keycode == Key.Escape)
		{
			Paused ^= true;
		}
	}
}
