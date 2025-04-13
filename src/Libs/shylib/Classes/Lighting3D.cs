using Godot;
using System;
using Godot.Collections;
using System.IO;


[Tool]
[GlobalClass, Icon("uid://dob8ycfdib6x8")]
public partial class Lighting3D : Node3D
{
	static public PackedScene Default = GD.Load<PackedScene>("res://src/Resources/Lighting/Scenes/default.tscn");

	[Export] public PackedScene Lighting = Default;

	public Node CurrentLighting;
	public WorldEnvironment SceneWorld;
	public DirectionalLight3D SceneSun;

	public WorldEnvironment World;
	public DirectionalLight3D Sun;

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

	public void DisposeLightings() 
	{
		World?.QueueFree(); 
		Sun?.QueueFree(); 
		World = null;
		Sun = null;
	}

	public void Reset() 
	{
		if (!Visible) return;

		this.ClearChildren();

		Node CurrentLighting = Lighting.Instantiate();

		SceneWorld = CurrentLighting.FindChild<WorldEnvironment>("T");
		SceneSun = CurrentLighting.FindChild<DirectionalLight3D>("T");

		World = SceneWorld?.Duplicate<WorldEnvironment>();
		Sun = SceneSun?.Duplicate<DirectionalLight3D>();

		if (World is not null) AddChild(World);
		if (Sun is not null) AddChild(Sun);

		// environment = World.Environment;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		DisposeLightings();
   	}

	public void OnVisibilityChanged() 
	{
		if (Sun is not null) Sun.Visible = Visible;

		if (Visible) {
			World?.QueueFree();
			World = SceneWorld?.Duplicate<WorldEnvironment>();
			if (World is not null) AddChild(World);
		}
		else {
			World?.QueueFree();
			World = null;
		}
	}

	public override void _Ready()
	{
		Connect(SignalName.VisibilityChanged, new Callable(this, MethodName.OnVisibilityChanged));

		base._Ready();
		if (Visible) Reset();
	}
}
