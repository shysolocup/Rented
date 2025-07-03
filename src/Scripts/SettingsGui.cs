using System;
using System.Linq;
using System.Threading.Tasks;
using CoolGame;
using Godot;

public partial class SettingsGui : Control
{
	private bool inSettings = false;

	public GameSetting CurrentTab;

	private Node Settings;
	private Node Tabs;

	private Player Player;

	public override void _Ready()
	{
		Player = this.GetGameNode<Player>("%Player");
		Hide();

		GetChild<Button>(1).Pressed += () => InSettings = false;

		Settings = GetNode("%SettingsContainer");
		Tabs = GetNode("%TabButtons");

		CurrentTab = Settings.GetChild<GameSetting>(0);

		foreach (var tab in Tabs.GetChildren().Cast<Button>())
		{
			tab.Pressed += () =>
			{
				if (CurrentTab is not null)
				{
					CurrentTab.EmitSignal(GameSetting.SignalName.Disabled);
					CurrentTab._Disabled();
					CurrentTab.Hide();
				}

				CurrentTab = Settings.GetNode<GameSetting>((string)tab.Name);

				if (CurrentTab is not null)
				{
					CurrentTab.EmitSignal(GameSetting.SignalName.Enabled);
					CurrentTab._Enabled();
					CurrentTab.Show();
				}
			};
		}

		foreach (var page in Settings.GetChildren().Cast<Control>()) page.Hide();

		CurrentTab.Show();
	}

	public bool InSettings {
		get => inSettings;

		set {
			if (inSettings != value)
			{
				inSettings = value;

				if (value)
				{
					Show();
				}
				else
				{
					Game.Delay(0.4f, Hide);
				}
			}
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		Modulate = Modulate.Lerp(inSettings ? Colors.White : Colors.Transparent, 1 / 0.3f * (float)delta);
	}
}
