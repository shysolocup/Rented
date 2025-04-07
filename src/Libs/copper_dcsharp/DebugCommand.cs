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