using Godot;

[GlobalClass]
public partial class _BuiltInCommands : GodotObject
{
	public void Init()
	{

		// Set noise


		DebugConsole.AddCommandSetvar(, "set_noise", _set_noise, this, DebugCommand.ParameterType.Float, "Sets the player's \"Noise\" value");


		// Clear


		DebugConsole.AddCommand(, "clear", DebugConsole.ClearLog, DebugConsole, new Array{}, "Clears the console.");


		// Show stats


		DebugConsole.AddCommandSetvar(, "show_stats", _show_stats, this, DebugCommand.ParameterType.Bool, "Sets whether the stats in the top left is visible.", _get_stats_shown);


		// Show log


		DebugConsole.AddCommandSetvar(, "show_mini_log", _show_log, this, DebugCommand.ParameterType.Bool, "Sets whether the mini log in the top right is visible.", _get_log_shown);


		// Exec
		var cfgs = new Array{};
		foreach(Variant file in ListFilesInDirectory("user://cfg"))
		{
			var fileSplit = file.Split(".");
			if(fileSplit[ - 1] == "cfg")
			{
				cfgs.Append(fileSplit[0]);
			}
		}

		var autoexec = FileAccess.Open("user://cfg/autoexec.cfg", FileAccess.ModeFlags.Read);
		if(autoexec != null)
		{_Exec("autoexec");
		}


		DebugConsole.AddCommand(, "exec", _exec, this, new Array{DebugCommand.Parameter.New("cfg", DebugCommand.ParameterType.Options, cfgs), }, "Executes the given cfg file, from top to bottom.");


		// Open cfg directory


		DebugConsole.AddCommand(, "open_cfg_dir", _open_cfg_dir, this, new Array{}, "Opens the directory where cfg files are put, if it exists.");

		var monitors = DebugConsole.GetConsole().Monitors.Keys();

		// Show/hide monitor


		DebugConsole.AddCommand(, "monitor", DebugConsole.SetMonitorVisible, DebugConsole, new Array{
					DebugCommand.Parameter.New("monitor", DebugCommand.ParameterType.Options, monitors), 
					DebugCommand.Parameter.New("visible", DebugCommand.ParameterType.Bool), 
					}, "Sets whether a particular stat monitor is visible.");

		var commands = DebugConsole.GetConsole().Commands.Keys();
		commands.Sort();

		// Help


		DebugConsole.AddCommand(, "help", _help, this, new Array{DebugCommand.Parameter.New("command", DebugCommand.ParameterType.Options, commands), }, "Use to get help on any particular command.");
	}

	public Array ListFilesInDirectory(Godot.Variant path)
	{
		var files = new Array{};
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
				else if(!file.BeginsWith("."))
				{
					files.Append(file);
				}
			}

			dir.ListDirEnd();

			return files;
		}
		return new Array{};
	}

	protected void _SetNoise(Godot.Variant value)
	{
		var game = dc_entry.Gamenode;
		game.Noise = value;
	}

	protected void _ShowStats(Godot.Variant value)
	{
		var console = DebugConsole.GetConsole();
		console.ShowStats = value;
		console.Stats.Visible = true;
	}

	protected bool _GetStatsShown()
	{
		return DebugConsole.GetConsole().ShowStats;
	}

	protected void _ShowLog(Godot.Variant value)
	{
		var console = DebugConsole.GetConsole();
		console.ShowMiniLog = value;
		if(!console.CommandField.Visible)
		{
			console.MiniLog.Visible = true;
		}
	}

	protected bool _GetLogShown()
	{
		return DebugConsole.GetConsole().ShowMiniLog;
	}

	protected void _Exec(Godot.Variant file)
	{
		var commands = FileAccess.Open("user://cfg/" + file + ".cfg", FileAccess.ModeFlags.Read).GetAsText().Split("\r\n");
		var commandCount = 0;
		foreach(PackedStringArray command in commands)
		{
			if(command.Replace(" ", "") != "")
			{
				DebugConsole.GetConsole().ProcessCommand(command);
				commandCount += 1;
			}
		}
		DebugConsole.Log("File " + file + ".cfg ran " + Str(commandCount) + " commands.");
	}

	protected void _OpenCfgDir()
	{
		Godot.OS.ShellOpen(Godot.ProjectSettings.GlobalizePath("user://cfg"));
	}

	protected void _Help(Godot.Variant command)
	{
		var helpText = DebugConsole.GetConsole().Commands[command].HelpText;
		DebugConsole.Log(command + " - " + (( helpText != "" ? helpText : "There is no help available." )));
	}


}