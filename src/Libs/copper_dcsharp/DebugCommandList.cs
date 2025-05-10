using System.Linq;
using Godot;
using Godot.Collections;

/// <summary>
/// <see cref="DebugCommandFunctions"/>
/// </summary>

public static class DebugCommandList
{

	public static void Init(DebugConsole console)
	{
		InitConfig(console);
		
		var funcs = new DebugCommandFunctions();

		#region shader


		new DebugCommand {
			Id = "shader",
			HelpText = "Toggles if a shader is visible",

			Parameters = [
				new DebugParameter {
					Name = "name",
					Type = DebugParameterType.String,
					Required = true
				},

				new DebugParameter {
					Name = "value",
					Type = DebugParameterType.Bool,
					Required = true
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.shader)
		}.AddTo(console);


		#endregion
		#region tpcamto


		new DebugCommand {
			Id = "tpcamto",
			HelpText = 
@"teleports the player camera to an object in the scene
eg:
	- tpto %Player
	- tpto %Markers/Dish
",

			Parameters = [
				new DebugParameter {
					Name = "nodename",
					Type = DebugParameterType.String,
					Required = true
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.tpcamto)
		}.AddTo(console);


		#endregion
				#region tpcamto


		new DebugCommand {
			Id = "tpcharto",
			HelpText = 
@"teleports the player character to an object in the scene
eg:
	- tpto %PlayerCamera
	- tpto %Markers/Dish
",

			Parameters = [
				new DebugParameter {
					Name = "nodename",
					Type = DebugParameterType.String,
					Required = true
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.tpcharto)
		}.AddTo(console);


		#endregion
		#region act


		new DebugCommand {
			Id = "act",
			HelpText = 

@"run a command from a list of quick lib actions
eg:
	- act camera Default
	- act dialogue convo_test
	- act light Day
	- act marker Dish			
",

			Parameters = [
				new DebugParameter {
					Name = "type",
					Type = DebugParameterType.Options,
					Options = [
						"camera",
						"dialogue",
						"place",
						"room",
						"light",
						"marker"
					],
					Required = true
				},

				new DebugParameter {
					Name = "name",
					Type = DebugParameterType.String,
					Required = true
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.act)
		}.AddTo(console);


		#endregion
		#region stats


		new DebugCommand {
			Id = "stats",
			HelpText = "Toggles whether or not the performance stats in the top left should be visible or not.",

			Parameters = [
				new DebugParameter {
					Name = "show",
					Type = DebugParameterType.Bool,
					Required = true
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.showstats),
			GetFunction = new Callable(funcs, DebugCommandFunctions.MethodName.statsvisible)
		}.AddTo(console);


		#endregion
		#region fullbright


		new DebugCommand {
			Id = "fullbright",
			HelpText = "similar to unlit but it lights up the game",

			Parameters = [
				new DebugParameter {
					Name = "value",
					Type = DebugParameterType.Bool,
					Required = true
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
					Type = DebugParameterType.Bool,
					Required = true
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.unlit)
		}.AddTo(console);


		#endregion
		#region freecam


		new DebugCommand {
			Id = "freecam",
			HelpText = "Lets you look around freely for secrets.. not that you'll find any of course",

			Parameters = [
				new DebugParameter {
					Name = "value",
					Type = DebugParameterType.Bool,
					Required = true
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
					Type = DebugParameterType.Bool,
					Required = true
				}
			],

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.pause)
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
			Id = "minilog",
			HelpText = "Toggles whether or not the mini log in the top right should be visible or not.",

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.minilog),
			GetFunction = new Callable(funcs, DebugCommandFunctions.MethodName.minilogvisible),

			Parameters = [
				new DebugParameter {
					Name = "value",
					Type = DebugParameterType.Bool,
					Required = true
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

			Function = new Callable(funcs, DebugCommandFunctions.MethodName.opencfgdir),
		}.AddTo(console);

		
		#endregion
		#region monitor


		var monitors = new Array<string>(console.Monitors.Keys) { "all" };


		new DebugCommand {
			Id = "monitor",
			HelpText = "Toggles the visibility of a stat monitor.",

			Parameters = [
				new DebugParameter {
					Name = "monitor",
					Type = DebugParameterType.Options,
					Options = monitors,
					Required = true
				},

				new DebugParameter {
					Name = "value",
					Type = DebugParameterType.Bool,
					Required = true
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
					Options = commands,
					Required = true
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
				else if (!file.StartsWith('.')) files.Add(file);
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
