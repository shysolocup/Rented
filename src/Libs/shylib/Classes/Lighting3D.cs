using Godot;
using System;
using Godot.Collections;
using System.IO;
using System.Linq;


[Tool]
[GlobalClass, Icon("uid://dob8ycfdib6x8")]
public partial class Lighting3D : Node3D
{
	static public string SceneDir = "res://src/Resources/Lighting/Scenes";

	static private Dictionary<string, PackedScene> SceneCache = new() {
		["Default"] = GD.Load<PackedScene>($"{SceneDir}/Default.tscn")
	};

	private PackedScene lighting = SceneCache["Default"];

	[Export] public PackedScene Lighting {
		get { return lighting; }
		set {
			if (value != lighting) {
				lighting = value;
				ResetApply();
			}
		}
	}

	public Node CurrentLighting;
	public WorldEnvironment SceneWorld;
	public DirectionalLight3D SceneSun;

	public WorldEnvironment World;
	public DirectionalLight3D Sun;

	[ExportToolButton("Reset / Apply")] 
	public Callable ResetCall => Callable.From(ResetApply);

	public Lighting3D DisposeLightings() 
	{
		World?.QueueFree(); 
		Sun?.QueueFree(); 
		World = null;
		Sun = null;
		return this;
	}

	public Lighting3D ResetApply() 
	{
		if (Visible) {
			this.ClearChildren();

			Node CurrentLighting = Lighting.Instantiate();

			SceneWorld?.QueueFree();
			SceneSun?.QueueFree();
			
			SceneWorld = null;
			SceneSun = null;

			SceneWorld = CurrentLighting.FindChild<WorldEnvironment>("T");
			SceneSun = CurrentLighting.FindChild<DirectionalLight3D>("T");

			World = SceneWorld?.Duplicate<WorldEnvironment>();
			Sun = SceneSun?.Duplicate<DirectionalLight3D>();

			if (World is not null) AddChild(World);
			if (Sun is not null) AddChild(Sun);
		}

		return this;
	}

	public PackedScene LoadFromScene(string scene) 
	{
		PackedScene ps = SceneCache.TryGetValue(scene, out PackedScene value) ? value : GD.Load<PackedScene>($"{SceneDir}/{scene}.tscn");
		if (!SceneCache.ContainsKey(scene)) SceneCache.Add(scene, ps);
		return ps;
	}

	public Lighting3D LoadAndSetFromScene(string scene)
	{
		Lighting = LoadFromScene(scene);
		return this;
	}

	public Lighting3D SetFromScene(PackedScene scene)
	{
		lighting = scene;
		return this;
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
		if (Visible) ResetApply();
	}
}
