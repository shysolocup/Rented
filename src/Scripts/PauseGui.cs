using Godot;
using System;
using CoolGame;
using System.Threading.Tasks;

public partial class PauseGui : Control
{
	private bool paused = false;

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


	public override void _Ready()
	{
		Hide();
		Player = GetNode<Player>("%Player");
		// World = GetNode<WorldEnvironment>("%World");

		VBoxContainer Container = GetChild<VBoxContainer>(0).GetChild<VBoxContainer>(1);

		// Resume
		Container.GetChild<Button>(0).Pressed += () => {
			Paused = false;
		};

		// Settings
		Container.GetChild<Button>(1).Pressed += () => {

		};
		
		// Skip Dialogue
		Container.GetChild<Button>(2).Pressed += () => {
			
		};

		// Quit
		Container.GetChild<Button>(3).Pressed += () => {
			GetTree().Quit();
		};

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
