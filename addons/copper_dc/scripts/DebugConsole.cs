using CoolGame;
using Godot;
using System;
using System.Linq;

[GlobalClass]
public partial class DebugConsole : CanvasLayer
{


	public partial class Monitor : GodotObject
	{

		public string Id;
		public string DisplayName;
		public Variant Value;
		public bool Visible;

		public Monitor(string id, string displayName, Variant value, bool visible)
		{
			this.Id = id;
			this.DisplayName = displayName;
			this.Value = value;
			this.Visible = visible;
		}
	}

	public Godot.Collections.Array ConsoleLog = new Godot.Collections.Array{};
	public Godot.Collections.Dictionary<string, DebugCommand> Commands = new Godot.Collections.Dictionary<string, DebugCommand>{};
	public Godot.Collections.Dictionary<string, Monitor> Monitors = new Godot.Collections.Dictionary<string, Monitor>{};
	public Godot.Collections.Array History = new Godot.Collections.Array{};
	public int CurrentHistory =  - 1;

	public bool PauseOnOpen = false;
	public bool ShowStats = false;
	public bool ShowMiniLog = false;
	public bool ShowNoise = false;

	public LineEdit CommandField;
	public Control ConsolePanel;

	public Control CommandHintsPanel;
	public ScrollContainer CommandHintsParent;
	public RichTextLabel CommandHintsLabel;
	public Control CommandHintHeader;
	public RichTextLabel CommandHintHeaderLabel;

	public Label Stats;
	public ScrollContainer MiniLog;
	public ScrollContainer LogField;
	public VScrollBar LogScrollBar;
	public VScrollBar MiniLogScrollBar;


	# region Overrides and signals
	public override async void _Ready()
	{
		CommandField = GetNode<LineEdit>("%Command Field");
		ConsolePanel = GetNode<Control>("%ConsolePanel");
		CommandHintsPanel = GetNode<Control>("%Command Hints Panel");
		CommandHintsParent = GetNode<ScrollContainer>("%Command Hints");
		CommandHintsLabel = GetNode<RichTextLabel>("%Command Hints/RichTextLabel");
		CommandHintHeader = GetNode<Control>("%Command Hint Header");
		CommandHintHeaderLabel = GetNode<RichTextLabel>("%Command Hint Header/RichTextLabel");
		Stats = GetNode<Label>("%Stats");
		MiniLog = GetNode<ScrollContainer>("%Mini Log");
		LogField = GetNode<ScrollContainer>("%Log");
		LogScrollBar = LogField.GetVScrollBar();
		MiniLogScrollBar = MiniLog.GetVScrollBar();
		HideConsole();
		LogScrollBar.Connect("changed", Callable.From(_OnScrollbarChanged));


		// Register built-in monitors
		AddMonitor("fps", "FPS");
		AddMonitor("process", "Process", false);
		AddMonitor("physics_process", "Physics Process", false);
		AddMonitor("navigation_process", "Navigation Process", false);
		AddMonitor("static_memory", "Static Memory", false);
		AddMonitor("static_memory_max", "Static Memory Max", false);
		AddMonitor("objects", "Objects", false);
		AddMonitor("nodes", "Nodes", false);
		AddMonitor("noise", "Noise", false);

		SetupCfg();
		SetPauseOnOpen(true);


		// Register built-in commands
		await ToSignal(GetTree().CreateTimer(0.05), "Timeout");
		DebugCommandList.Init(this);
	}

	protected void _OnScrollbarChanged()
	{
		LogField.ScrollVertical = (int)LogScrollBar.MaxValue;
	}

	public override void _Process(double delta)
	{
		if(Stats.Visible) {
			if(IsMonitorVisible("fps")) UpdateMonitor("fps", Performance.GetMonitor(Performance.Monitor.TimeFps));
			if(IsMonitorVisible("process")) UpdateMonitor("process", Mathf.Snapped(Performance.GetMonitor(Performance.Monitor.TimeProcess), 0.001));
			if(IsMonitorVisible("physics_process")) UpdateMonitor("physics_process", Mathf.Snapped(Godot.Performance.GetMonitor(Godot.Performance.Monitor.TimePhysicsProcess), 0.001));
			if(IsMonitorVisible("navigation_process")) UpdateMonitor("navigation_process", Mathf.Snapped(Godot.Performance.GetMonitor(Godot.Performance.Monitor.TimeNavigationProcess), 0.001));
			if(IsMonitorVisible("static_memory")) UpdateMonitor("static_memory", Mathf.Snapped(Godot.Performance.GetMonitor(Godot.Performance.Monitor.MemoryStatic), 0.001));
			if(IsMonitorVisible("static_memory_max")) UpdateMonitor("static_memory_max", Mathf.Snapped(Godot.Performance.GetMonitor(Godot.Performance.Monitor.MemoryStaticMax), 0.001));
			if(IsMonitorVisible("objects")) UpdateMonitor("objects", Godot.Performance.GetMonitor(Godot.Performance.Monitor.ObjectCount));
			if(IsMonitorVisible("nodes")) UpdateMonitor("nodes", Godot.Performance.GetMonitor(Godot.Performance.Monitor.ObjectNodeCount));
			if(IsMonitorVisible("noise"))UpdateMonitor("noise", Game.Instance.Noise);

			Stats.Text = "";

			foreach (Monitor monitor in Monitors.Values) {
				if(monitor.Visible)
				{
					if((object)monitor.Value == null) monitor.Value = "unset";
					else monitor.Value = (string)monitor.Value;

					Stats.Text += monitor.DisplayName + ": " + monitor.Value + "\n";
				}
			}
		}
	}

	public override async void _Input(InputEvent @event)
	{

		// Open debug
		if (!ConsolePanel.Visible && @event.IsActionPressed("toggle_debug")) {
			ShowConsole();
			_OnCommandFieldTextChanged(CommandField.Text);

			// This is stupid but it works
			await ToSignal(GetTree().CreateTimer(0.02), "Timeout");
			CommandField.GrabFocus();
		}

		// Close debug
		else if (ConsolePanel.Visible && @event.IsActionPressed("ui_cancel")) {
			HideConsole(ShowStats, ShowMiniLog);
		}

		// Enter command
		else if (ConsolePanel.Visible && @event.IsActionPressed("ui_text_submit") && CommandField.Text.Length > 0) {
			Log("> " + CommandField.Text);
			ProcessCommand(CommandField.Text);
			CommandField.Clear();
		}

		// Back in history
		else if(ConsolePanel.Visible && @event.IsActionPressed("ui_up")) {
			if(History.Count > 0 && CurrentHistory !=  - 1)
			{
				if(CurrentHistory > 0)
				{
					CurrentHistory -= 1;
				}
				CommandField.Text = (string)History[CurrentHistory];
				await ToSignal(GetTree(), "ProcessFrame");
				CommandField.SetCaretColumn(CommandField.Text.Length);
			}
		}

		// Forward in history
		else if(ConsolePanel.Visible && @event.IsActionPressed("ui_down")) {
			if(History.Count > 0 && CurrentHistory < History.Count - 1) {
				CurrentHistory += 1;
				CommandField.Text = (string)History[CurrentHistory];
				await ToSignal(GetTree(), "ProcessFrame");
				CommandField.SetCaretColumn(CommandField.Text.Length);
			}
			else if(CurrentHistory == History.Count - 1) {
				CommandField.Text = "";
				CurrentHistory = History.Count;
				await ToSignal(GetTree(), "ProcessFrame");
				CommandField.SetCaretColumn(CommandField.Text.Length);
			}
		}

		// Tab completion
		else if(ConsolePanel.Visible && _IsTabPress(@event)) {
			_AttemptAutocompletion();
		}
	}

	public bool _IsTabPress(InputEvent @event = null) {
		if (@event.GetType() != typeof(InputEventKey)) { return false; }
		
		InputEventKey KeyEvent = @event as InputEventKey;
		return KeyEvent.Keycode == Key.Tab && KeyEvent.Pressed && !KeyEvent.Echo;
	}

	public void _AttemptAutocompletion()
	{
		// Populate the hints label with words we could autocomplete
		_OnCommandFieldTextChanged(CommandField.Text);

		// Gather the first word of each hint, stripping the [url] wrappers
		var hints = new Godot.Collections.Array();
		foreach(string hint in CommandHintsLabel.Text.Split("\n"))
		{
			hints.Add(hint.GetSlice(']', 1).GetSlice('[', 0).GetSlice(' ', 0));
		}
		hints = hints.Slice(0, -1);

		// Find the common prefix to all hints
		var common_prefix = "";

		if(hints.Count > 0) {
			foreach(int i in GD.Range(1000)) {
				if (!hints.All((string h) => {	
					return h.Length > i && h[i] == ((string)hints[0])[i];
				})) {
					common_prefix += ((string)hints[0])[i];
					break;
				}
			}
		}

		if(!CommandHintsLabel.Visible || common_prefix == "") {
			return;
		}

		if(hints.Count == 1) {
			common_prefix += " ";
			// Only one hint, so complete the whole word

		}
		
		// Replace the last word, if any, with `common_prefix`
		
		var r = new RegEx();
		r.Compile(@"(\w+)?$");

		// "Any non-whitespace characters until the end"
		var new_text = r.Sub(CommandField.Text, common_prefix);

		CommandField.Text = new_text;
		CommandField.CaretColumn = new_text.Length;
		
		_OnCommandFieldTextChanged(new_text);
	}

	protected void _OnCommandFieldTextChanged(string new_text)
	{
		var commandHints = new Godot.Collections.Array();
		var commandSplit = new Godot.Collections.Array<string>(((string)new_text).Split(" "));
		
		var commandID = commandSplit[0];

		if(commandSplit.Count > 1 && Commands.Keys.Contains(commandID)) {
			CommandHintsParent.Visible = true;
			CommandHintsLabel.Visible = true;
			CommandHintsPanel.Visible = true;
			CommandHintHeader.Visible = true;
			CommandHintsLabel.Text = "";


			// Get parameters filled
			var parameterCount = 0;
			var readingString = false;
			foreach(string word in commandSplit)
			{
				if (word.StartsWith("\""))
				{
					if(!readingString)
					{parameterCount += 1;
					}
					if(word != "\"")
					{
						if(!word.EndsWith("\""))
						{
							readingString = true;
						}
					}
					else
					{
						readingString = !readingString;
					}
				}
				else if (word.EndsWith("\""))
				{
					readingString = false;
				}
				else
				{
					if(!readingString)
					{parameterCount += 1;
					}
				}
			}
			parameterCount -= 2;
			DebugCommand cmd = (DebugCommand)Commands[commandID];

			CommandHintHeaderLabel.Text = _GetParameterText(cmd, parameterCount);

			if (parameterCount < cmd.Parameters.Count) {
				var options = cmd.Parameters[parameterCount].Options;
				
				if (options.Count > 0) {
					foreach(string option in options) {
						if (option.StartsWith(commandSplit[commandSplit.Count - 1]))
						{
							CommandHintsLabel.Text += "[url]" + option + "[/url]\n";
						}
					}
				}
			}
		}

		else {
			var sortedCommands = new Godot.Collections.Array<string>(Commands.Keys);
			sortedCommands.Sort();

			foreach(string command in sortedCommands)
			{
				if(command.StartsWith(commandID))
				{
					commandHints.Append(Commands[command]);
				}
			}

			CommandHintHeader.Visible = false;

			if (commandHints.Count > 0) {
				CommandHintsParent.Visible = true;
				CommandHintsLabel.Visible = true;
				CommandHintsPanel.Visible = true;
				CommandHintsLabel.Text = "";
				foreach(DebugCommand command in commandHints)
				{
					CommandHintsLabel.Text += "[url=" + command.Id + "]" + _GetParameterText(command) + "[/url]\n";
				}
			}

			else
			{
				CommandHintsParent.Visible = false;
				CommandHintsLabel.Visible = false;
				CommandHintsPanel.Visible = false;
			}
		}
	}

	protected void _OnCommandHintsMetaClicked(string meta)
	{
		var commandSplit = new Godot.Collections.Array<string>(CommandField.Text.Split(" "));
		commandSplit.Append(meta);
		var newText = "";

		foreach(string i in commandSplit)
		{
			newText += i + " ";
		}
		CommandField.Text = newText;
		CommandField.CaretColumn = CommandField.Text.Length;
		_OnCommandFieldTextChanged(CommandField.Text);


		# endregion

	}
	
	# region Commands processing
	
	protected string _GetParameterText(DebugCommand command, int currentParameter =  - 1)
	{
		var text = command.Id;
		var isHeader = currentParameter < command.Parameters.Count && currentParameter >= 0;

		foreach (DebugParameter parameter in command.Parameters) {

			if (isHeader && parameter.Name == command.Parameters[currentParameter].Name) {
				string typename = parameter.Name;
				text += " [b]<" + parameter.Name + ": " + parameter.Type.ToString() + ">[/b]";
			}
			
			else {
				text += " <" + parameter.Name + ": " + parameter.Type.ToString() + ">";
			}
			if ((object)command.GetFunction != null)
			{
				var value = (string)command.GetFunction.Call();
				if(value != null) {
					text += " === " + value;
				}
			}
		}
		return text;
	}

	public void LogTypeError(DebugParameter parameter)
	{
		LogError($"TypeError: Parameter {parameter.Name} should be an {parameter.Type.GetType().Name}, but an incorrect value was passed.");
	}

	public Godot.Collections.Dictionary<bool, Godot.Collections.Array> Options = new Godot.Collections.Dictionary<bool, Godot.Collections.Array>{
		{true, new Godot.Collections.Array{
			true, "on", "1", 
		}},
		{false, new Godot.Collections.Array{
			false, "off", "0", 
		}}
	};

	public void ProcessCommand(string command)
	{

		// Avoid duplicating history entries
		if(History.Count == 0 || command != (string)History.Last())
		{
			History.Append(command);
			CurrentHistory = History.Count;
		}

		// Splits command
		var commandSplit = command.Split(" ");

		// Checks if command is valid
		if(!Commands.Keys.Contains(commandSplit[0])) {
			LogError("Command not found: " + commandSplit[0]);
			return;
		}

		// Keeps track of current parameter being read
		var commandData = Commands[commandSplit[0]];
		var currentParameter = 0;


		// Checks that function is not lambda
		if (commandData.Function.Method == "<anonymous lambda>") {
			LogError("Command function must be named.");
			Log(commandData.Function.Method);
			return;
		}
		var commandFunction = commandData.Function.Method + "(";
		var currentString = "";

		var required = commandData.Parameters.Where( p => p.Required );

		if (commandData.Parameters.Count() > currentParameter) {
			LogError($"ParamError: Command \"{commandData.Id}\" requires {commandData.Parameters.Count} parameters, but too many were given.");
			return ;
		}

		// Iterates through split list
		foreach (int i in GD.Range(commandSplit.Length)) {
			if(i == 0) {
				continue;
			}
			
			else if (commandSplit[i] == "") {
				continue;
			}

			var currentParameterObj = commandData.Parameters[currentParameter];

			// Int parameter
			if (currentParameterObj.Type == DebugParameterType.Int) {
				if (!commandSplit[i].IsValidInt()) {
					LogTypeError(currentParameterObj);
					return;
				}
				commandFunction += commandSplit[i] + ",";
				currentParameter += 1;
			}

			// Float parameter
			else if (currentParameterObj.Type == DebugParameterType.Float) {
				if (!commandSplit[i].IsValidFloat()) {
					LogTypeError(currentParameterObj);
					return;
				}
				commandFunction += commandSplit[i] + ",";
				currentParameter += 1;
			}

			// String parameter
			else if (currentParameterObj.Type == DebugParameterType.String) {
				var word = commandSplit[i];

				if(word.StartsWith("\"")) {

					if(word.EndsWith("\"")) {

						if(word == "\"") {
							if(currentString == "") currentString += "\" ";

							else {
								commandFunction += currentString + "\",";
								currentParameter += 1;
							}
						}

						else {
							commandFunction += word + ",";
							currentParameter += 1;
						}
					}

					else if (currentString != "") {
						LogError("FuckinError: Cannot create a string within a string.");
						return;
					}

					else {
						currentString += word + " ";
					}
				}

				else if (currentString != "") {

					if(word.EndsWith("\"")) {
						currentString += word;
						commandFunction += currentString + ",";
						currentString = "";
						currentParameter += 1;
					}

					else currentString += word + " ";
				}
				
				else {
					commandFunction += "\"" + word + "\",";
					currentParameter += 1;
				}
			}

			// Bool parameter
			else if (currentParameterObj.Type == DebugParameterType.Bool) {
				var value = commandSplit[i].ToLower();
				
				if(!Options[true] && !Options[false].Contains(value).Contains(value)) {
					LogTypeError(currentParameterObj);
					return ;
				}
				value = ( options[true].Contains(value) ? true : false );
				commandFunction += value + ",";
				currentParameter += 1;
			}

			// Options parameter
			else if(currentParameterObj.Type == DebugCommand.ParameterType.Options)
			{
				if(currentParameterObj.Options.IsEmpty())
				{
					DebugConsole.LogError("Parameter \"" + currentParameterObj.Name + "\" is meant to have options, but none were set.");
					return ;
				}
				if(!currentParameterObj.Options.Has(commandSplit[i]))
				{
					DebugConsole.LogError("\"" + commandSplit[i] + "\"" + " is not a valid option for parameter \"" + currentParameterObj.Name + "\".");
					return ;
				}
				commandFunction += "\"" + commandSplit[i] + "\",";
				currentParameter += 1;
			}

			// Other
			else
			{
				DebugConsole.LogError("Parameter \"" + currentParameterObj.Name + "\" received an invalid value.");
				return ;
			}
		}


		// Checks if all parameters are entered
		if(commandData.Parameters.Size() != currentParameter)
		{
			DebugConsole.LogError("Command " + commandData.Id + " requires " + Str(commandData.Parameters.Size()) + " parameters, but only " + Str(currentParameter) + " were given.");
			return ;
		}

		commandFunction += ")";

		var expression = Expression.New();
		var error = expression.Parse(commandFunction);
		if(error)
		{
			DebugConsole.LogError("Parsing error: " + Error.ToString(error));
			return ;
		}

		expression.Execute(new Array{}, commandData.FunctionInstance);


		# endregion

	}# region Logging
	public static void Log(Godot.Variant message)
	{

		// Add to log
		GetConsole().ConsoleLog.Append(message);
		_UpdateLog();


		// Print to Godot output
		GD.Print(Str(message));
	}

	public static void LogError(Godot.Variant message)
	{

		// Add to log
		GetConsole().ConsoleLog.Append("[color=red]" + Str(message) + "[/color]");
		_UpdateLog();


		// Print to Godot output
		GD.PrintErr(Str(message));
	}

	public static void ClearLog()
	{
		GetConsole().ConsoleLog.Clear();
		_UpdateLog();


		# endregion

	}# region Creating commands
	public static void AddCommand(String id, Callable function, Godot.Object functionInstance, Array parameters = new Array{}, String helpText = "", Godot.Variant getFunction = null)
	{
		GetConsole().Commands[id] = DebugCommand.New(id, function, functionInstance, parameters, helpText, getFunction);
	}

	public static void AddCommandSetvar(String id, Callable function, Godot.Object functionInstance, DebugCommand.ParameterType type, String helpText = "", Godot.Variant getFunction = null)
	{
		GetConsole().Commands[id] = DebugCommand.New(id, function, functionInstance, new Array{
							DebugCommand.Parameter.New("value", type), 
							}, helpText, getFunction);
	}

	public static void AddCommandObj(Godot.DebugCommand command)
	{
		GetConsole().Commands[command.Id] = command;


		# endregion

	}# region Removing commands
	public static bool RemoveCommand(String id)
	{
		return GetConsole().Commands.Erase(id);
	}

	public static void RemoveCommands(Array<String> ids)
	{
		foreach(String id in ids)
		{
			RemoveCommand(id);


			# endregion

		}
	}# region Monitors
	public static void AddMonitor(Godot.Variant id, Godot.Variant displayName, bool visible = true)
	{
		if(id.Contains(" "))
		{
			DebugConsole.LogError("Monitor id \"" + id + "\"" + "needs to be a single word.");
			return ;
		}
		else if(GetConsole().Monitors.Keys().Contains(id))
		{

		}
		else
		{
			GetConsole().Monitors[id] = Monitor.New(id, displayName, null, Visible);
		}
	}

	public static void UpdateMonitor(Godot.Variant id, Godot.Variant value)
	{
		if(!GetConsole().Monitors.Keys().Contains(id))
		{
			DebugConsole.LogError("Monitor " + id + " does not exist.");
		}
		else
		{
			GetConsole().Monitors[id].Value = value;
		}
	}

	public static bool IsMonitorVisible(Godot.Variant id)
	{
		var monitors = GetConsole().Monitors;
		if(!Monitors.Keys().Contains(id))
		{return false;
		}
		else
		{return Monitors[id].Visible;
		}
	}

	public static void SetMonitorVisible(Godot.Variant id, Godot.Variant visible)
	{
		if(!GetConsole().Monitors.Keys().Contains(id))
		{
			DebugConsole.LogError("Monitor " + id + " does not exist.");
		}
		else
		{
			GetConsole().Monitors[id].Visible = Visible;


			# endregion

		}
	}
	# region Console managing
	public static Godot.DebugConsole GetConsole()
	{
		return (Godot.Engine.GetMainLoop()).Root.GetNode("/root/debug_console");
	}

	public static void HideConsole(bool showStats = false, bool showMiniLog = false)
	{
		var console = GetConsole();
		console.ConsolePanel.Visible = false;
		console.Stats.Visible = ShowStats;
		console.MiniLog.Visible = ShowMiniLog;
		await ToSignal(console.GetTree().CreateTimer(0.01), "Timeout");
		console.MiniLog.ScrollVertical = console.MiniLogScrollBar.MaxValue;

		Godot.Input.MouseMode = Godot.Input.MouseMode.MouseModeCaptured;

		if(console.PauseOnOpen)
		{console.GetTree().Paused = false;
		}
	}

	public static void ShowConsole()
	{
		var console = GetConsole();
		console.ConsolePanel.Visible = true;
		console.Stats.Visible = true;
		console.MiniLog.Visible = false;

		Godot.Input.MouseMode = Godot.Input.MouseMode.MouseModeVisible;

		if(console.PauseOnOpen)
		{console.GetTree().Paused = true;
		}
	}

	public static bool IsConsoleVisible()
	{
		var console = GetConsole();
		if(GodotObject.IsInstanceValid(console))
		{
			return console.GetNode("ConsolePanel").Visible;
		}
		else
		{
			return false;
		}
	}

	public static void SetPauseOnOpen(bool pause)
	{
		GetConsole().PauseOnOpen = pause;
	}

	protected static void _UpdateLog()
	{
		var console = GetConsole();
		var logText = "";
		foreach(Variant line in console.ConsoleLog)
		{
			logText += Str(line) + "\n";
		}

		console.LogField.GetNode("MarginContainer/Log Content").Text = logText;
		console.MiniLog.GetNode("MarginContainer/Log Content").Text = "[right]" + logText;
	}


	# endregion
	public static void SetupCfg()
	{
		DirAccess.MakeDirAbsolute("user://cfg");
	}


	//var file = FileAccess.open("user://cfg/autoexec.cfg", FileAccess.WRITE)
}