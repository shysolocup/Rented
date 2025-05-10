using Godot;
using Godot.Collections;
[Tool]
[GlobalClass, Icon("uid://dob8ycfdib6x8")]
public partial class Lighting3D : Node3D
{
	[Signal] public delegate void LightingChangedEventHandler();
	[Signal] public delegate void LightingDisabledEventHandler();

	static private readonly string SceneDir = "res://src/Resources/Lighting/Packed";

	private static Dictionary<string, PackedScene> SceneCache = new() {
		["Default"] = LoadFromScene("Default")
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

				if (CurrentLighting is null || !IsInstanceValid(CurrentLighting)) {
					DebugConsole.LogError($"LightingError: failed to instantiate lighting scene \"{lighting.ResourcePath}\"");
					return this;
				}

				SceneWorld?.QueueFree();
				SceneSun?.QueueFree();
				
				SceneWorld = null;
				SceneSun = null;

				SceneWorld = CurrentLighting.FindChild<WorldEnvironment>("T");
				SceneSun = CurrentLighting.FindChild<DirectionalLight3D>("T");

				World = SceneWorld?.Duplicate<WorldEnvironment>();
				Sun = SceneSun?.Duplicate<DirectionalLight3D>();

				if (World is not null && IsInstanceValid(World)) {
					GD.Print($"Set World to {World}");
					AddChild(World);
				}

				if (Sun is not null && IsInstanceValid(Sun)) {
					GD.Print($"Set Sun to {Sun}");
					AddChild(Sun);
				}

				EmitSignalLightingChanged();
			}
			else {
				DebugConsole.LogError($"LightingError: invalid lighting scene \"{lighting.ResourcePath}\"");
			}
		}

		return this;
	}

	public static PackedScene LoadFromScene(string scene, bool useCache = true) 
	{
		if (ResourceLoader.Exists($"{SceneDir}/{scene}.tscn")) {
			PackedScene ps = (useCache && SceneCache.TryGetValue(scene, out PackedScene value)) ? value : ResourceLoader.Load<PackedScene>($"{SceneDir}/{scene}.tscn", "", ResourceLoader.CacheMode.Replace);
			if (!SceneCache.ContainsKey(scene) || !useCache) SceneCache[scene] = ps;
			return ps;
		}
		else {
			DebugConsole.LogError($"LightingError: cannot find or failed to load scene \"{SceneDir}/{scene}.tscn\"");
			return null;
		}
	}

	public Lighting3D LoadAndSetFromScene(string scene, bool useCache = true)
	{
		if (LoadFromScene(scene, useCache) is PackedScene packed) {
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