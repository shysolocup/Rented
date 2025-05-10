using Godot;
using CoolGame;

/// <summary>
/// <see cref="DebugCommandList"/>
/// </summary>

public partial class DebugCommandFunctions : GodotObject 
{
	#region shader

	public void shader(string name, bool value) {
		Game.Instance.GetGameNode<ColorRect>($"%ScreenShaders/{name}").Visible = value;
	}

	#endregion
	#region tpcamto

	public void tpcamto(string nodename) {
		Camera3D a = Game.Instance.GetGameNode<Camera3D>("%PlayerCamera");
		Node3D b = Game.Instance.GetGameNode<Node3D>(nodename);

		a.Position = b.Position;
		a.Rotation = b.Rotation;
	}

	#endregion
	#region tpcharto

	public void tpcharto(string nodename) {
		Player a = Game.Instance.GetGameNode<Player>("%Player");
		Node3D b = Game.Instance.GetGameNode<Node3D>(nodename);

		a.Position = b.Position;
		a.Rotation = b.Rotation;
	}

	#endregion
	#region showstats


	public void showstats(bool value) 
	{
		var console = DebugConsole.GetConsole();
		console.Stats.Visible = value;

		GD.Print(console.Stats.Visible);
	}


	#endregion
	#region statsvisible


	public bool statsvisible() => DebugConsole.GetConsole().Stats.Visible;


	#endregion
	#region setnoise


	public void setnoise(float value) => Game.Instance.Noise = value;


	#endregion
	#region clear


	public void clear() => DebugConsole.ClearLog();


	#endregion
	#region minilog


	public void minilog(bool value) 
	{
		var console = DebugConsole.GetConsole();

		console.ShowMiniLog = value;

		if (!DebugConsole.CommandField.Visible) {
			console.MiniLog.Visible = true;
		}
	}


	#endregion
	#region act

	public async void act(string type, string name) {
		if (type == "camera") {
			Game.Instance.GetGameNode("%Cameras").GetNode<Camera3D>( (name == "Default") ? "PlayerCamera" : name).MakeCurrent();
		}

		else if (type == "dialogue") {
			Player player = Game.Instance.GetGameNode<Player>("%Player");
			await DebugConsole.HideConsole();
			await player.PlayDialogue(name);
		}

		else if (type == "place") {
			PlaceController pc = Game.Instance.GetGameNode<PlaceController>("%PlaceController");
			pc.Place = PlaceController.Places[name];
		}

		else if (type == "room") {
			Game.Instance?.LoadRoom(name);
		}

		else if (type == "light") {
			Game.Instance?.Lighting.LoadAndSetFromScene(name).ResetApply();
		}

		else if (type == "marker") {
			Player player = Game.Instance?.GetGameNode<Player>("%Player");
		
			if (player is null) return;

			player.GlobalPosition = Game.Instance.GetGameNode<Marker3D>($"%Markers/{name}").GlobalPosition;
		}

		else if (type == "map") {
			Game.Instance?.Map.LoadAndSetFromScene(name).ResetApply();
		}
	}


	#endregion
	#region minilogvisible


	public bool minilogvisible() => DebugConsole.GetConsole().ShowMiniLog;


	#endregion
	#region exec


	public void exec(string file) => DebugCommandList._Exec(file);


	#endregion
	#region opencfgdir


	public void opencfgdir() => DebugCommandList._OpenCfgDir();


	#endregion
	#region monitor


	public void monitor(string monitor, bool value) => DebugConsole.GetConsole().Monitors[monitor].Visible = value;

	
	#endregion
	#region help
	

	public void help(string command)
	{
		var helpText = DebugConsole.GetConsole().Commands[command].HelpText;
		DebugConsole.Log($"{command} - { ((helpText != "") ? helpText : "There is no help available.") }");
	}


	#endregion
	#region fullbright


	public void fullbright(bool value) {
		
		Lighting3D lighting = Game.Instance?.Lighting;
		if (lighting is null) return;

		if (value) {
			lighting.ResetApply(Lighting3D.LoadFromScene("Fullbright"));
		}
		else {
			lighting.ResetApply();
		}
	}


	#endregion
	#region unlit


	public void unlit(bool value) {
		Game.Instance.GetNode<Control>("/root/Game/Dump/%ScreenShaders").Visible = value;
		Game.Instance.Lighting.Visible = !value;
	}


	#endregion
	#region freecam


	public async void freecam(bool value) {
		Player player = Game.Instance?.GetNode<Player>("%Player");
		
		if (player is null) return;

		await DebugConsole.HideConsole();
		player.Freecam = value;
	}


	#endregion freecam
	#region pause


	public void pause(bool value) => Game.Instance.GetTree().Paused = value;


	#endregion pause
}
