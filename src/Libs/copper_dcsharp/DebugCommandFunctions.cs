using Godot;
using CoolGame;
using Godot.Collections;
using System.Linq;

/// <summary>
/// <see cref="DebugCommandList"/>
/// </summary>

public partial class DebugCommandFunctions : GodotObject 
{
	#region tp

	public void tp(float x, float y, float z)
	{
		Player player = Game.Instance.GetGameNode<Player>("%Player");
		player.GlobalPosition = new Vector3(x, y, z);
	}

	#endregion
	#region shader

	public void shader(string name, bool value)
	{
		Game.Instance.GetGameNode<ColorRect>($"%ScreenShaders/{name}").Visible = value;
	}

	#endregion
	#region tpcamto

	public void tpcamto(string nodename) {
		Camera3D a = Game.Instance.GetGameNode<Camera3D>("%PlayerCamera");
		Node3D b = Game.Instance.GetGameNode<Node3D>(nodename);

		a.Position = b.Position;
	}

	#endregion
	#region tpcharto

	public void tpcharto(string nodename) {
		Player a = Game.Instance.GetGameNode<Player>("%Player");
		Node3D b = Game.Instance.GetGameNode<Node3D>(nodename);

		a.Position = b.Position;
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
	#region actoptions


	public Array<string> actoptions(Array<string> opts)
	{
		string type = opts[1];

		if (type == "collect")
		{
			return [.. Game.Instance.GetGameNode<Inventory>("%Inventory").GetOtherItems().Select(item => item.Name)];
		}

		if (type == "throw" || type == "equip")
		{
			return [.. Game.Instance.GetGameNode<Inventory>("%Inventory").GetInventoryItems().Select(item => item.Name)];
		}

		if (type == "camera")
		{
			return [.. Game.Instance.GetGameNode("%Cameras").GetChildren().Select(child => child.Name), "Default"];
		}

		else if (type == "dialogue")
		{
			return [.. DialogueData.Scenes.Keys];
		}

		/*else if (type == "place") {
			PlaceController pc = Game.Instance.GetGameNode<PlaceController>("%PlaceController");
			pc.Place = PlaceController.Places[name];
		}*/

		/*else if (type == "room")
		{

		}*/

		else if (type == "light")
		{
			var dir = DirAccess.Open(Lighting3D.SceneDir);
			return [.. dir.GetFiles().Select(file => file.Replace(".tscn", ""))];
		}

		else if (type == "marker")
		{
			return [.. Game.Instance.GetGameNode("%Markers").GetChildren().Select(child => child.Name)];
		}

		else if (type == "map")
		{
			var dir = DirAccess.Open(MapController.SceneDir);
			return [.. dir.GetFiles().Select(file => file.Replace(".tscn", ""))];
		}

		return [];
	}


	#endregion
	#region act

	public async void act(string type, string name)
	{
		/* adds given item from inventory */
		if (type == "collect")
		{
			Game.Instance.GetGameNode<Inventory>("%Inventory").Collect(name);
		}

		/* removes given item from inventory */
		if (type == "throw")
		{
			Game.Instance.GetGameNode<Inventory>("%Inventory").Throw(name);
		}

		/* equips given item */
		if (type == "equip")
		{
			Game.Instance.GetGameNode<Inventory>("%Inventory").Equip(name);
		}

		/* goes to a camera */
		if (type == "camera")
		{
			Game.Instance.GetGameNode("%Cameras").GetNode<Camera3D>((name == "Default") ? "PlayerCamera" : name).MakeCurrent();
		}

		/* plays a dialog line */
		else if (type == "dialogue")
		{
			Player player = Game.Instance.GetGameNode<Player>("%Player");
			await DebugConsole.HideConsole();
			await player.PlayDialogue(name);
		}

		/* sets the discord status */
		/*else if (type == "place") {
			PlaceController pc = Game.Instance.GetGameNode<PlaceController>("%PlaceController");
			pc.Place = PlaceController.Places[name];
		}*/

		/* loads a given lighting preset */
		else if (type == "light")
		{
			Game.Instance?.Lighting.LoadAndSetFromScene(name).ResetApply();
		}

		/* tps the player to a marker */
		else if (type == "marker")
		{
			Player player = Game.Instance?.GetGameNode<Player>("%Player");

			if (player is null) return;

			player.GlobalPosition = Game.Instance.GetGameNode<Marker3D>($"%Markers/{name}").GlobalPosition;
		}

		/* loads a given map */
		else if (type == "map")
		{
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
		// Game.Instance.GetNode<Control>("/root/Game/Dump/%ScreenShaders").Visible = value;
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
	#region setnoise


	public void setnoise(int value) {
		Game.Instance.Noise = value;
	}


	#endregion freecam
	#region pause


	public void pause(bool value) => Game.Instance.GetTree().Paused = value;


	#endregion pause
}
