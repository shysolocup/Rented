using CoolGame;
using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

[GlobalClass]
public partial class DebugConsole : CanvasLayer
{

	public Godot.Collections.Array ConsoleLog = new Godot.Collections.Array{};
	public Godot.Collections.Dictionary<string, DebugCommand> Commands = new Godot.Collections.Dictionary<string, DebugCommand>{};
	public Godot.Collections.Dictionary<string, DebugMonitor> Monitors = new Godot.Collections.Dictionary<string, DebugMonitor>{};
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

		await HideConsole();
		
		LogScrollBar.Connect("changed", Callable.From(_OnScrollbarChanged));


		// Register built-in monitors

		await Task.Run(() => DebugMonitorList.Init(this));


		SetupCfg();
		SetPauseOnOpen(true);


		// Register built-in commands
		await ToSignal(GetTree().CreateTimer(0.05), Timer.SignalName.Timeout);

		DebugCommandList.Init(this);
	}

	protected void _OnScrollbarChanged()
	{
		LogField.ScrollVertical = (int)LogScrollBar.MaxValue;
	}

	public override void _Process(double delta)
	{
		if (Stats.Visible) {

			Stats.Text = "";

			foreach (DebugMonitor monitor in Monitors.Values) {
				if (monitor.Visible) {
					monitor.Update();

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
			await ToSignal(GetTree().CreateTimer(0.02), Timer.SignalName.Timeout);
			CommandField.GrabFocus();
		}

		// Close debug
		else if (ConsolePanel.Visible && @event.IsActionPressed("ui_cancel")) {
			await HideConsole(ShowStats, ShowMiniLog);
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
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
				CommandField.SetCaretColumn(CommandField.Text.Length);
			}
		}

		// Forward in history
		else if(ConsolePanel.Visible && @event.IsActionPressed("ui_down")) {
			if(History.Count > 0 && CurrentHistory < History.Count - 1) {
				CurrentHistory += 1;
				CommandField.Text = (string)History[CurrentHistory];
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
				CommandField.SetCaretColumn(CommandField.Text.Length);
			}
			else if(CurrentHistory == History.Count - 1) {
				CommandField.Text = "";
				CurrentHistory = History.Count;
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
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
		foreach(string hint in CommandHintsLabel.Text.Split("\n")) {
			hints.Add(hint.GetSlice(']', 1).GetSlice('[', 0).GetSlice(' ', 0));
		}
		hints = hints.Slice(0, -1);

		GD.Print(hints);

		// Find the common prefix to all hints
		var common_prefix = "";

		if (hints.Count > 0) {
			foreach(int i in GD.Range(1000)) {
				if (!hints.All((string h) => {	
					return h.Length > i && h[i] == ((string)hints[0])[i];
				})) {
					common_prefix += ((string)hints[0])[i];
					break;
				}
			}
		}

		GD.Print(common_prefix);

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
			DebugCommand cmd = Commands[commandID];

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
				
				foreach(DebugCommand command in commandHints) {
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

	public static void LogTypeError(DebugParameter parameter)
	{
		LogError($"TypeError: Parameter \"{parameter.Name}\" should be a [{parameter.Type}], but an incorrect value was passed.");
	}

	public Godot.Collections.Dictionary<bool, Godot.Collections.Array> BoolOptions = new Godot.Collections.Dictionary<bool, Godot.Collections.Array> {
		{true, new Godot.Collections.Array{
			"true", "on", "1", 
		}},

		{false, new Godot.Collections.Array{
			"false", "off", "0", 
		}}
	};

	public void ProcessCommand(string command)
	{

		// Avoid duplicating history entries
		if (History.Count == 0 || command != (string)History.Last()) {
			History.Append(command);
			CurrentHistory = History.Count;
		}

		// Splits command
		var commandSplit = command.Split(" ");


		// Checks if command is valid
		if(!Commands.ContainsKey(commandSplit[0])) {
			LogError("GuhError: Command not found: " + commandSplit[0]);
			return;
		}


		// Keeps track of current parameter being read
		var commandData = Commands[commandSplit[0]];
		var currentParameter = 0;

		var commandFunction = commandData.Function.Method + "(";
		var currentString = "";

		var required = commandData.Parameters.Where( p => p.Required );

		if (commandData.Parameters.Count < currentParameter) {
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
				var value = commandSplit[i].ToLower().Trim();
				
				if ( !BoolOptions[true].Contains(value) && !BoolOptions[false].Contains(value) ) {
					LogTypeError(currentParameterObj);
					return;
				}

				value = BoolOptions[true].Contains(value) ? "true" : "false";
				commandFunction += value + ",";
				currentParameter += 1;
			}

			// Options parameter
			else if(currentParameterObj.Type == DebugParameterType.Options)
			{
				if (currentParameterObj.Options.Count == 0) {
					LogError("ParamError: Parameter \"" + currentParameterObj.Name + "\" is meant to have options, but none were set.");
					return ;
				}
				if (!currentParameterObj.Options.Contains(commandSplit[i])) {
					LogError($"ParamError: \"{commandSplit[i]}\" is not a valid option for parameter \"{currentParameterObj.Name}\".");
					return ;
				}
				commandFunction += "\"" + commandSplit[i] + "\",";
				currentParameter += 1;
			}

			// Other
			else {
				LogError($"ParamError: Parameter \"{currentParameterObj.Name}\" recieved an invalid value.");
				return ;
			}
		}


		// Checks if all parameters are entered
		if (currentParameter < required.Count()) {
			LogError($"ParamError: Command {commandData.Id} requires {commandData.Parameters.Count} parameters but only {currentParameter} were given.");
			return ;
		}

		commandFunction += ")";

		GD.Print(commandFunction);

		var expression = new Expression();
		var error = expression.Parse(commandFunction);
		
		if (error != Error.Ok) {
			LogError($"ParsingError: {expression.GetErrorText()}");
			return;
		}

		GD.Print((object)expression.Execute(this.GetBlank<Godot.Collections.Array>(), commandData.Function.Target));


		# endregion

	}
	
	# region Logging
	
	public static void Log(string message)
	{

		// Add to log
		GetConsole().ConsoleLog.Append(message);
		_UpdateLog();


		// Print to Godot output
		GD.Print(message);
	}

	public static void LogError(string message)
	{

		// Add to log
		GetConsole().ConsoleLog.Append("[color=red]" + message + "[/color]");
		_UpdateLog();


		// Print to Godot output
		GD.PrintErr(message);
	}

	public static void ClearLog()
	{
		GetConsole().ConsoleLog.Clear();
		_UpdateLog();


		# endregion

	}
	
	/*# region Creating commands
	
	public static void AddCommand(String id, Callable function, GodotObject functionInstance, Array parameters = new Array{}, String helpText = "", Godot.Variant getFunction = null)
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
	}
	*/

	# region Monitors
	
	/*public static void AddMonitor(string id, string displayName, bool visible = true)
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
	}*/

	# region Console managing
	public static DebugConsole GetConsole()
	{
		return (Engine.GetMainLoop() as SceneTree).Root.GetNode("/root/debug_console") as DebugConsole;
	}

	public static async Task<bool> HideConsole(bool showStats = false, bool showMiniLog = false)
	{
		var console = GetConsole();
		console.ConsolePanel.Visible = false;
		console.Stats.Visible = console.ShowStats;
		console.MiniLog.Visible = console.ShowMiniLog;

		await console.ToSignal(console.GetTree().CreateTimer(0.01), Timer.SignalName.Timeout);

		console.MiniLog.ScrollVertical = (int)console.MiniLogScrollBar.MaxValue;

		Input.MouseMode = Input.MouseModeEnum.Captured;

		if (console.PauseOnOpen) {
			console.GetTree().Paused = false;
		}

		return true;
	}

	public static void ShowConsole()
	{
		var console = GetConsole();

		console.ConsolePanel.Visible = true;
		console.Stats.Visible = true;
		console.MiniLog.Visible = false;

		Input.MouseMode = Input.MouseModeEnum.Visible;

		if(console.PauseOnOpen) {
			console.GetTree().Paused = true;
		}
	}

	public static bool IsConsoleVisible()
	{
		var console = GetConsole();
		if (IsInstanceValid(console)) {
			return console.GetNode<Control>("ConsolePanel").Visible;
		}

		else {
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

		foreach(string line in console.ConsoleLog) {
			logText += line + "\n";
		}

		console.LogField.GetNode<RichTextLabel>("MarginContainer/Log Content").Text = logText;
		console.MiniLog.GetNode<RichTextLabel>("MarginContainer/Log Content").Text = "[right]" + logText;
	}


	# endregion
	public static void SetupCfg()
	{
		DirAccess.MakeDirAbsolute("user://cfg");
	}


	//var file = FileAccess.open("user://cfg/autoexec.cfg", FileAccess.WRITE)
	#endregion
}
