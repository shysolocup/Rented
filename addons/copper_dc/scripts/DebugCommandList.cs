using System;
using System.Linq;
using System.Reflection;
using Godot;
using CoolGame;

public static class DebugCommandList
{

	public static void Init(DebugConsole console)
	{

		#region CONFIG


		var cfgs = new Godot.Collections.Array();
		var dir = DirAccess.Open("user://cfg");

		foreach(string file in ListFilesInDirectory("user://cfg"))
		{
			var fileSplit = file.Split(".");

			if (fileSplit.Last() == "cfg") {
				cfgs.Append(fileSplit[0]);
			}
		}

		var autoexec = FileAccess.Open("user://cfg/autoexec.cfg", FileAccess.ModeFlags.Read);

		if(autoexec != null) {
			_Exec("autoexec");
		}

		var monitors = DebugConsole.GetConsole().Monitors.Keys();


		#endregion
		#region show_stats


		new DebugCommand(console) {
			Id = "show_stats",
			HelpText = "Toggles whether or not the performance stats in the top left should be visible or not.",

			Parameters = new Godot.Collections.Array<DebugParameter> {
				new DebugParameter {
					Name = "show?",
					Type = DebugParameterType.Bool
				}
			},

            Function = Callable.From((bool value) => {
				var console = DebugConsole.GetConsole();
				console.ShowStats = value;
				console.Stats.Visible = true;
			}),

			GetFunction = Callable.From<bool>(() => {
                return console.ShowStats;
            })
		};


		#endregion
		#region set_noise


		new DebugCommand(console) {
			Id = "set_noise",
			HelpText = "Sets the player's \"Noise\" value.",

			Parameters = new Godot.Collections.Array<DebugParameter> {
				new DebugParameter {
					Name = "value",
					Type = DebugParameterType.Float
				}
			},

            Function = Callable.From((float value) => {
				Game.Instance.Noise = value;
			})
		};


		#endregion
		#region clear


		new DebugCommand(console) {
			Id = "clear",
			HelpText = "Clears the console.",

			Function = Callable.From(() => {
				console.ClearLog();
			})
		};


		#endregion
		#region show_mini_log


		new DebugCommand(console) {
			Id = "show_mini_log",
			HelpText = "Toggles whether or not the mini log in the top right should be visible or not.",

			Function = Callable.From((bool value) => {
				var console = DebugConsole.GetConsole();

				console.ShowMiniLog = value;

				if (!console.CommandField.Visible) {
					console.MiniLog.Visible = true;
				}
			}),

			GetFunction = Callable.From<bool>(() => {
                return console.ShowMiniLog;
            })
		};


		#endregion
		#region exec


		new DebugCommand(console) {
			Id = "exec",
            HelpText = "Executes the given cfg file.",

			Function = Callable.From((string file) => {
				_Exec(file);
			})
		};


		#endregion
		#region open_cfg_dir


		new DebugCommand(console) {
			Id = "open_cfg_dir",
            HelpText = "Opens the directory where cfg files are put, if it exists.",

			Function = Callable.From(() => {
				_OpenCfgDir();
			})
		};

		
		#endregion
		#region monitor


		new DebugCommand(console) {
			Id = "monitor",
            HelpText = "Toggles the visibility of a stat monitor.",

			Parameters = new Godot.Collections.Array<DebugParameter> {
				new DebugParameter {
					Name = "monitor",
					Type = DebugParameterType.Options,
					Options = monitors
				},

				new DebugParameter {
					Name = "value",
					Type = DebugParameterType.Bool
				}
			},

			Function = Callable.From((DebugConsole.Monitor monitor, bool value) => {
				DebugConsole.SetMonitorVisible(monitor, value);
			})
		};


		#endregion
		#region help


		new DebugCommand(console) {
			Id = "help",
            HelpText = "Use to get help on any particular command.",

			Parameters = new Godot.Collections.Array<DebugParameter> {
				new DebugParameter {
					Name = "command",
					Type = DebugParameterType.Options,
					Options = DebugConsole.GetConsole().Commands.Keys.Sort()
				}
			},

			Function = Callable.From((string command) => {
				var helpText = DebugConsole.GetConsole().Commands[command].HelpText;
				DebugConsole.Log($"{command} - { ((helpText != "") ? helpText : "There is no help available.") }");
			})
		};
	}


	#endregion
	#region CLASS FUNCTIONS


	public static Godot.Collections.Array ListFilesInDirectory(string path)
	{
		var files = new Godot.Collections.Array{};
		if(DirAccess.Open("user://").DirExists(path))
		{
			var dir = DirAccess.Open(path);
			dir.ListDirBegin();

			while(true)
			{
				var file = dir.GetNext();
				if(file == "")
				{
					break;
				}
				else if(!file.StartsWith("."))
				{
					files.Append(file);
				}
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
			if(command.Replace(" ", "") != "")
			{
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
}