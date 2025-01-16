using System;
using System.ComponentModel;
using Godot;

[GlobalClass]
public partial class DebugCommand : GodotObject
{
	public string Id;
	public Godot.Collections.Array Parameters;
	public Callable Function;
	public string HelpText;
	public Variant GetFunction;
	public Variant nullvar = new Variant();

	public DebugCommand(String id, Callable function, Godot.Collections.Array parameters = null, String helpText = "")
	{
		Id = id;
		Parameters = (parameters != null) ? parameters : new Godot.Collections.Array();
		Function = function;
		HelpText = helpText;
	}

	public partial class Parameter : GodotObject
	{

		public string Name;
		public ParameterType Type;
		public Godot.Collections.Array Options;

		public Parameter(String name, ParameterType type, Godot.Collections.Array options = null)
		{
			this.Name = name;
			this.Type = type;
			this.Options = (options != null) ? options : new Godot.Collections.Array();
		}
	}

	public enum ParameterType {Int, Float, String, Bool, Options}


}