using System;
using System.ComponentModel;
using Godot;

[GlobalClass]
public partial class DebugCommand : GodotObject
{
	public string Id;
	public Godot.Collections.Array<DebugParameter> Parameters = new Godot.Collections.Array<DebugParameter>();
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
	public Godot.Collections.Array<string> Options = new Godot.Collections.Array<string>();
}


public enum DebugParameterType {Int, Float, String, Bool, Options}