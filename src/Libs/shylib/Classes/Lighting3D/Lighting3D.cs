using Godot;
using System;
using Godot.Collections;
using System.IO;


[Tool]
[GlobalClass, Icon("res://src/Libs/shylib/Images/Lighting3D")]
public partial class Lighting3D : Node3D
{
	static public PackedScene Default = GD.Load<PackedScene>("res://src/Libs/shylib/Classes/Lighting3D/guh.tscn");

	private Godot.Environment environment;

	[Export] public WorldEnvironment World;
	[Export] public DirectionalLight3D Sun;

	[Export] public Godot.Environment Environment {
		get { return environment; }
		set {
			if (value != environment) {
				environment = value;
				World.Environment = Environment;
			}
		}
	}

	[ExportToolButton("Reset")] 
	public Callable ResetCall => Callable.From(Reset);

	public void Reset() 
	{
		if (World is not null) World.Free();
		if (Sun is not null) Sun.Free();

		Node def = Default.Instantiate();
		AddChild(def);

		// World = def.GetChild<WorldEnvironment>(1);
		// Sun = def.GetChild<DirectionalLight3D>(2);

		// AddChild(World);
		// AddChild(Sun);

		// environment = World.Environment;
	}

	public override bool _Set(StringName property, Variant valueVar)
	{
		GD.Print(property);

		if (property == "Visible" && valueVar.Obj is bool value) {
			if (!value) {
				GD.Print(World);
				World.Environment = null;
				World.CameraAttributes = null;
			}
		}
		return true;
	}

	public override void _Ready()
	{
		base._Ready();
		Reset();
	}
}
