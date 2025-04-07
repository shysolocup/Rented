using System;
using Godot;
using Godot.Collections;

public partial class DebugParameter : GodotObject
{
	public string Name;
	public bool Required = true;
	public DebugParameterType Type;
	public Array<string> Options = new Array<string>();
	public Callable CallOptions;
}