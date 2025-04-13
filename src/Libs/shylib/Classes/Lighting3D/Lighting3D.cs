using Godot;
using System;
using Godot.Collections;
using System.IO;


[Tool]
[GlobalClass, Icon("res://src/Libs/shylib/Images/Lighting3D")]
public partial class Lighting3D : Node3D
{
	static public PackedScene Default = GD.Load<PackedScene>("res://src/Resources/Lighting/Scenes/default.tscn");

	public PackedScene Lighting = Default;

	[Export] public WorldEnvironment World;
	[Export] public DirectionalLight3D Sun;

	/*[Export] public Lighting3DEnvironment Environment {
		get { return environment; }
		set {
			if (value != environment) {
				environment = value;
				// World.Environment = Environment;
			}
		}
	}*/

	[ExportToolButton("Reset")] 
	public Callable ResetCall => Callable.From(Reset);

	public void Reset() 
	{
		World?.Free();
		Sun?.Free();

		Node def = Default.Instantiate();

		World = def.GetChild<WorldEnvironment>(1);
		Sun = def.GetChild<DirectionalLight3D>(2);

		AddChild(World);
		AddChild(Sun);

		// environment = World.Environment;
	}

	public override void _Ready()
	{
		VisibilityChanged += () => {
			if (Visible) {
				
			}
			else {
				World.Environment = null;
				World.CameraAttributes = null;
			}
			GD.Print(World);
		};

		base._Ready();
		Reset();
	}
}
