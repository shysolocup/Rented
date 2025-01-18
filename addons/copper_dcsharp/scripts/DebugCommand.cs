using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class DebugCommand : GodotObject
{
	public string Id;
	public Array<DebugParameter> Parameters = new Array<DebugParameter>();
	public string HelpText;

	public Callable Function;
	public Callable GetFunction;

	public void AddTo(DebugConsole console) {
		console.Commands.Add(Id, this);
	}
}


public partial class DebugParameter : GodotObject
{
	public string Name;
	public bool Required = true;
	public DebugParameterType Type;
	public Array<string> Options = new Array<string>();
}


public enum DebugParameterType {Int, Float, String, Bool, Options}