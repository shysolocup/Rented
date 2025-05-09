using Godot;
using Godot.Collections;


[Tool]
[GlobalClass, Icon("uid://dob8ycfdib6x8")]
public partial class Lighting3D : Node3D
{
	[Signal] public delegate void LightingChangedEventHandler();
	[Signal] public delegate void LightingDisabledEventHandler();

	static public string SceneDir = "res://src/Resources/Lighting/Scenes";

	static private Dictionary<string, PackedScene> SceneCache = new() {
		["Default"] = GD.Load<PackedScene>($"{SceneDir}/Default.tscn")
	};

	private PackedScene lighting = SceneCache["Default"];

	[Export] public PackedScene Lighting {
		get => lighting;
		set {
			if (value != lighting) {
				lighting = value;
				ResetApply();
			}
		}
	}

	[Export(PropertyHint.Enum, "None:1,EditorOnly:2,RuntimeOnly:3")] public int AutoVisibility = 1;

	public Node CurrentLighting;
	public WorldEnvironment SceneWorld;
	public DirectionalLight3D SceneSun;

	public WorldEnvironment World;
	public DirectionalLight3D Sun;

	[ExportToolButton("Reset / Apply")] 
	public Callable ResetCall => Callable.From(() => ResetApply());

	public Lighting3D DisposeLightings() 
	{
		World?.QueueFree();
		Sun?.QueueFree();
		World = null;
		Sun = null;
		return this;
	}

	public Lighting3D ResetApply(PackedScene lighting = null) 
	{
		if (Visible) {
			lighting ??= Lighting;

			if (IsInstanceValid(lighting)) {

				this.ClearChildren();

				using Node CurrentLighting = lighting.Instantiate();

				SceneWorld?.QueueFree();
				SceneSun?.QueueFree();
				
				SceneWorld = null;
				SceneSun = null;

				SceneWorld = CurrentLighting.FindChild<WorldEnvironment>("T");
				SceneSun = CurrentLighting.FindChild<DirectionalLight3D>("T");

				World = SceneWorld?.Duplicate<WorldEnvironment>();
				Sun = SceneSun?.Duplicate<DirectionalLight3D>();

				GD.Print($"Set World to {World}");
				GD.Print($"Set Sun to {Sun}");

				if (World is not null) AddChild(World);
				if (Sun is not null) AddChild(Sun);

				EmitSignalLightingChanged();
			}
		}

		return this;
	}

	public PackedScene LoadFromScene(string scene) 
	{
		if (ResourceLoader.Exists($"{SceneDir}/{scene}.tscn")) {
			PackedScene ps = SceneCache.TryGetValue(scene, out PackedScene value) ? value : GD.Load<PackedScene>($"{SceneDir}/{scene}.tscn");
			if (!SceneCache.ContainsKey(scene)) SceneCache.Add(scene, ps);
			return ps;
		}
		else {
			DebugConsole.LogError($"LightingError: cannot find or failed to load scene \"{SceneDir}/{scene}.tscn\"");
			return null;
		}
	}

	public Lighting3D LoadAndSetFromScene(string scene)
	{
		if (LoadFromScene(scene) is PackedScene packed) {
			Lighting = packed;
		}
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

		EmitSignalLightingDisabled();
	}

	public override async void _Ready()
	{
		base._Ready();
		_ = Connect(SignalName.VisibilityChanged, new Callable(this, MethodName.OnVisibilityChanged));
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		switch(AutoVisibility) {
			case 2: { // editor only
				if (Engine.IsEditorHint()) {
					Visible = true;
				}
				break;
			}

			case 3: { // runtime only
				if (!Engine.IsEditorHint()) {
					Visible = true;
				}
				break;
			}
		}

		if (Visible) ResetApply();
	}
}
