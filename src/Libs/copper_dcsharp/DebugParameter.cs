using Godot;
using Godot.Collections;

public partial class DebugParameter : GodotObject
{
	public string Name;
	public string HelpText = "No Description";
	public bool Required = true;
	public DebugParameterType Type;
	public Array<string> Options = [];
	public Callable? CallOptions;
}