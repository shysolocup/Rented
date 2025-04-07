using Godot;
using System;
using CoolGame;

public partial class PauseGui : Control
{
	private bool paused = false;

	public override void _Notification(int what)
	{
		if (what == NotificationWMWindowFocusOut) {
			Paused = true;
		}
	}

	public bool Paused {
		get { return paused; }

		set {
			if (paused != value) {
				paused = value;

				GetTree().Paused = value;

				if (value) {
					Player.ReleaseMouse(); 
					Show();
				}
				else {
					Player.CaptureMouse();
					Hide();
				}
			}
		}
	}
	private Player Player;

	public override void _Ready()
	{
		Hide();
		Player = GetNode<Player>("%Player");

		VBoxContainer Container = GetChild<VBoxContainer>(0);

		// Resume
		Container.GetChild<Button>(0).Pressed += () => {
			Paused = false;
		};

		// Settings
		Container.GetChild<Button>(1).Pressed += () => {

		};

		// Quit
		Container.GetChild<Button>(2).Pressed += () => {
			GetTree().Quit();
		};

	}

    public override void _UnhandledInput(InputEvent @event)
	{
		if (Player != null && @event is InputEventKey eventKey && eventKey.Pressed && eventKey.Keycode == Key.Escape) {
			Paused ^= true;
		}
	}
}
