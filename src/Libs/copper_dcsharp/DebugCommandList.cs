using System;
using System.Linq;
using Godot;
using CoolGame;
using Godot.Collections;

public partial class DebugCommandFunctions : GodotObject 
{

	public void show_stats(bool value) 
	{
		var console = DebugConsole.GetConsole();
		console.Stats.Visible = value;

		GD.Print(console.Stats.Visible);
	}

	public bool stats_visible() 
	{
		var console = DebugConsole.GetConsole();
		return console.Stats.Visible;
	}

	public void set_noise(float value)
	{
		Game.Instance.Noise = value;
	}

	public void clear()
	{
		DebugConsole.ClearLog();
	}

	public void show_mini_log(bool value) 
	{
		var console = DebugConsole.GetConsole();

		console.ShowMiniLog = value;

		if (!console.CommandField.Visible) {
			console.MiniLog.Visible = true;
		}
	}

	public void goto_camera(string camera)
	{
		Game.Instance.GetNode("%Cameras").GetNode<Camera3D>(camera).MakeCurrent();
	}

	public bool mini_log_visible() 
	{
		var console = DebugConsole.GetConsole();
		return console.ShowMiniLog;
	}

	public void exec(string file)
	{
		DebugCommandList._Exec(file);
	}

	public void open_cfg_dir()
	{
		DebugCommandList._OpenCfgDir();
	}

	public void monitor(string monitor, bool value)
	{
		DebugConsole console = DebugConsole.GetConsole();
		console.Monitors[monitor].Visible = value;
	}

	public void help(string command)
	{
		var helpText = DebugConsole.GetConsole().Commands[command].HelpText;
		DebugConsole.Log($"{command} - { ((helpText != "") ? helpText : "There is no help available.") }");
	}

	public async void dialogue(string line) {
		Player player = Game.Instance.GetNode<Player>("%Player");
		await DebugConsole.HideConsole();
		await player.PlayDialogue(line);
	}

	public void set_place(string place) {
		PlaceController pc = Game.Instance.GetNode<PlaceController>("%PlaceController");
		pc.Place = PlaceController.Places[place];
	}

	public void fullbright(bool value) {
		// Game.Instance.GameEnvironment = value ? GD.Load<Godot.Environment>("res://src/Resources/Skies/Fullbright.tres") : Game.Instance.DefaultGameEnvironment;
		// Game.Instance.GetNode<DirectionalLight3D>("%Sun").ShadowEnabled = !value;
	}

	public void unlit(bool value) {
		Game.Instance.Lighting.Visible = !value;
	}

	public void loadroom(string room) {
		Game.Instance.LoadRoom(room);
	}

	public void loadlighting(string scene) {
		Game.Instance.Lighting?.LoadAndSetFromScene(scene);
	}

	public async void freecam(bool value) {
		Player player = Game.Instance.GetNode<Player>("%Player");
		await DebugConsole.HideConsole();
		player.Freecam = value;
	}

	public void pause(bool value) {
		Game.Instance.GetTree().Paused = value;
	}
}

public static class DebugCommandList
{

	public static void Init(DebugConsole console)
	{
		InitConfig(console);
		
		var funcs = new DebugCommandFunctions();


		#region loadroom


		new DebugCommand {
			Id = "loadroom",
			HelpText = "Loads a room by string id",

			Parameters = [
				new DebugParameter {
					Name = "name",
					Type = DebugParameterType.String
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.loadroom)
		}.AddTo(console);


		#endregion
		#region set_place


		new DebugCommand {
			Id = "set_place",
			HelpText = "sets the place changing the discord rich presence",

			Parameters = [
				new DebugParameter {
					Name = "place",
					Type = DebugParameterType.String
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.set_place)
		}.AddTo(console);


		#endregion
		#region stats


		new DebugCommand {
			Id = "stats",
			HelpText = "Toggles whether or not the performance stats in the top left should be visible or not.",

			Parameters = [
				new DebugParameter {
					Name = "show",
					Type = DebugParameterType.Bool
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.show_stats),
			GetFunction = new Callable(funcs, DebugCommandFunctions.MethodName.stats_visible)
		}.AddTo(console);


		#endregion
		#region fullbright


		new DebugCommand {
			Id = "fullbright",
			HelpText = "similar to unlit but it lights up the game",

			Parameters = [
				new DebugParameter {
					Name = "value",
					Type = DebugParameterType.Bool
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.fullbright)
		}.AddTo(console);


		#endregion
		#region unlit


		new DebugCommand {
			Id = "unlit",
			HelpText = "Disables lighting effects",

			Parameters = [
				new DebugParameter {
					Name = "value",
					Type = DebugParameterType.Bool
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.unlit)
		}.AddTo(console);


		#endregion
		#region loadlighting


		new DebugCommand {
			Id = "loadlighting",
			HelpText = "Loads a lighting effect preset",

			Parameters = [
				new DebugParameter {
					Name = "lighting",
					Type = DebugParameterType.String
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.loadlighting)
		}.AddTo(console);


		#endregion
		#region freecam


		new DebugCommand {
			Id = "freecam",
			HelpText = "Lets you look around freely for secrets.. not that you'll find any of course",

			Parameters = [
				new DebugParameter {
					Name = "value",
					Type = DebugParameterType.Bool
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.freecam)
		}.AddTo(console);


		#endregion
		#region pause


		new DebugCommand {
			Id = "pause",
			HelpText = "pauses the game",

			Parameters = [
				new DebugParameter {
					Name = "value",
					Type = DebugParameterType.Bool
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.pause)
		}.AddTo(console);


		#endregion
		#region pause


		new DebugCommand {
			Id = "goto_camera",
			HelpText = "switches to a camera",

			Parameters = [
				new DebugParameter {
					Name = "camera",
					Type = DebugParameterType.String
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.goto_camera)
		}.AddTo(console);


		#endregion
		#region dialogue


		new DebugCommand {
			Id = "dialogue",
			HelpText = "Display a dialogue sequence",

			Parameters = [
				new DebugParameter {
					Name = "lines",
					Type = DebugParameterType.String,
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.dialogue)
		}.AddTo(console);


		#endregion
		#region set_noise


		new DebugCommand {
			Id = "set_noise",
			HelpText = "Sets the player's \"Noise\" value.",

			Parameters = [
				new DebugParameter {
					Name = "value",
					Type = DebugParameterType.Float
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.set_noise),
		}.AddTo(console);


		#endregion
		#region clear


		new DebugCommand {
			Id = "clear",
			HelpText = "Clears the console.",

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.clear)

		}.AddTo(console);


		#endregion
		#region mini_log


		new DebugCommand {
			Id = "mini_log",
			HelpText = "Toggles whether or not the mini log in the top right should be visible or not.",

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.show_mini_log),
			GetFunction = new Callable(funcs, DebugCommandFunctions.MethodName.mini_log_visible),

			Parameters = [
				new DebugParameter {
					Name = "value",
					Type = DebugParameterType.Bool
				}
			],

		}.AddTo(console);


		#endregion
		#region exec


		new DebugCommand {
			Id = "exec",
			HelpText = "Executes the given cfg file.",

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.exec),

		}.AddTo(console);


		#endregion
		#region cfg


		new DebugCommand {
			Id = "cfg",
			HelpText = "Opens the directory where cfg files are put, if it exists.",

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.open_cfg_dir),
		}.AddTo(console);

		
		#endregion
		#region monitor


		var monitors = new Array<string>(console.Monitors.Keys);


		new DebugCommand {
			Id = "monitor",
			HelpText = "Toggles the visibility of a stat monitor.",

			Parameters = [
				new DebugParameter {
					Name = "monitor",
					Type = DebugParameterType.Options,
					Options = monitors
				},

				new DebugParameter {
					Name = "value",
					Type = DebugParameterType.Bool
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.monitor),
		}.AddTo(console);


		#endregion
		#region help


		var commands = new Array<string>(console.Commands.Keys);
		commands.Sort();


		new DebugCommand {
			Id = "help",
			HelpText = "Use to get help on any particular command.",

			Parameters = [
				new DebugParameter {
					Name = "command",
					Type = DebugParameterType.Options,
					Options = commands
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.help),
		}.AddTo(console);
	}


	#endregion
	#region CLASS FUNCTIONS


	public static Array<string> ListFilesInDirectory(string path)
	{
		var files = new Array<string>();
		if(DirAccess.Open("user://").DirExists(path))
		{
			var dir = DirAccess.Open(path);
			dir.ListDirBegin();

			while (true) {
				var file = dir.GetNext();
				if (file == "") break;
				else if (!file.StartsWith(".")) files.Add(file);
			}

			dir.ListDirEnd();
		}
		return files;
	}

	public static void _Exec(string file)
	{
		var commands = FileAccess.Open("user://cfg/" + file + ".cfg", FileAccess.ModeFlags.Read).GetAsText().Split("\r\n");
		var commandCount = 0;
		foreach(string command in commands)
		{
			if (command.Replace(" ", "") != "") {
				DebugConsole.GetConsole().ProcessCommand(command);
				commandCount += 1;
			}
		}
		DebugConsole.Log($"File {file}.cfg ran {commandCount} commands");
	}

	public static void _OpenCfgDir()
	{
		OS.ShellOpen(ProjectSettings.GlobalizePath("user://cfg"));
	}

	#endregion

	public static void InitConfig(DebugConsole console) {
		#region CONFIG

		var cfgs = new Array<string>();
		var dir = DirAccess.Open("user://cfg");

		foreach(string file in ListFilesInDirectory("user://cfg"))
		{
			var fileSplit = file.Split(".");

			if (fileSplit.Last() == "cfg") {
				cfgs.Add(fileSplit[0]);
			}
		}

		var autoexec = FileAccess.Open("user://cfg/autoexec.cfg", FileAccess.ModeFlags.Read);

		if(autoexec != null) {
			_Exec("autoexec");
		}


		#endregion
	}
}
