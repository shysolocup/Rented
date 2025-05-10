using Godot;

public partial class PauseGui : Control
{
	private bool paused = false;

	public Button ResumeButton;
	public Button SkipButton;
	public Button SettingsButton;
	public Button QuitButton;

	private Player Player;
	/*private WorldEnvironment World;
	private CameraAttributesPhysical weirdCameraEffects = GD.Load("res://src/Resources/Ui/PauseCameraAttributes.tres") as CameraAttributesPhysical;
	private Godot.Environment weirdWorldEffects = GD.Load("res://src/Resources/Ui/PauseEffects.tres") as Godot.Environment;
	private CameraAttributes defaultCameraEffects = GD.Load("res://src/Resources/Skies/CameraAttributesPractical.tres") as CameraAttributesPractical;
	private Godot.Environment defaultWorldEffects = GD.Load("res://src/Resources/Skies/Default.tres") as Godot.Environment;*/

	public override void _Notification(int what)
	{
		if (what == NotificationWMWindowFocusOut) {
			Paused = true;
		}
	}

	public bool Paused {
		get => paused;

		set {
			if (paused != value) {
				paused = value;

				GetTree().Paused = value;

				if (value) {
					/*defaultWorldEffects = World.Environment;
					defaultCameraEffects = World.CameraAttributes;

					World.Environment = weirdWorldEffects;
					World.CameraAttributes = weirdCameraEffects;
					World.CameraAttributes.AutoExposureScale = 1;*/
					Player.ReleaseMouse(); 
					Show();
				}
				else if (!Player.InDialog) {
					/*World.Environment = defaultWorldEffects;
					World.CameraAttributes = defaultCameraEffects;*/
					Player.CaptureMouse();
					Hide();
				}
				else {
					Hide();
				}
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
		Hide();
		Player = this.GetGameNode<Player>("%Player");
		// World = this.GetGameNode<WorldEnvironment>("%World");

		VBoxContainer Container = GetChild<VBoxContainer>(0).GetChild<VBoxContainer>(1);

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

	public async override void _Process(double delta)
	{
		base._Process(delta);

		if (paused && !waiting) {
			/*float scale = World.CameraAttributes.AutoExposureScale;
			GD.Print(Math.Round(scale, 1));
			if (Math.Round(scale, 2) <= 0.3f && !rebound) rebound = true;
			if (Math.Round(scale) >= 1 && rebound) rebound = false;
			World.CameraAttributes.AutoExposureScale = this.Twlerp(scale, rebound ? 1 : 0.02f, 1/500f, delta);
			waiting = true;
			await Task.Delay(100);
			waiting = false;*/
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (Player != null && !DebugConsole.IsConsoleVisible() && @event is InputEventKey eventKey && eventKey.Pressed && eventKey.Keycode == Key.Escape) {
			Paused ^= true;
		}
	}
}
