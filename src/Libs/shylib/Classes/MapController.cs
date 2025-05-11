using Godot;
using Godot.Collections;


[Tool]
<<<<<<< HEAD
[GlobalClass, Icon("uid://dob8ycfdib6x8")]
=======
[GlobalClass, Icon("uid://bu4xjum6vtr11")]
>>>>>>> guh
public partial class MapController : Node3D
{
	[Signal] public delegate void MapChangedEventHandler();

	static private readonly string SceneDir = "res://src/Resources/Maps";

	private static Dictionary<string, PackedScene> SceneCache = new() {
<<<<<<< HEAD
		["Test"] = LoadFromScene("Test")
=======
		["Test"] = ResourceLoader.Load<PackedScene>($"{SceneDir}/Test.tscn", "", ResourceLoader.CacheMode.Replace)
>>>>>>> guh
	};

	private PackedScene mapScene = SceneCache["Test"];

	[Export] public PackedScene MapScene {
		get => mapScene;
		set {
			if (value != mapScene) {
				mapScene = value;
				ResetApply();
			}
		}
	}

<<<<<<< HEAD
    public Node Map;
=======
	public Node Map;
>>>>>>> guh

	[ExportToolButton("Reset / Apply")] 
	public Callable ResetCall => Callable.From(() => ResetApply());

	public MapController DisposeMap() 
	{
		Map?.QueueFree();
		Map = null;
		return this;
	}

	public MapController ResetApply(PackedScene map = null) 
	{
<<<<<<< HEAD
        map ??= MapScene;

        if (IsInstanceValid(map)) {

            DisposeMap();

            Map = map.Instantiate();

            if (Map is not null && IsInstanceValid(Map)) {
                GD.Print($"Set Map to {Map}");

                AddChild(Map);

                EmitSignalMapChanged();
            }
            else {
                DebugConsole.LogError($"MapError: failed to instantiate map scene \"{map.ResourcePath}\"");
            }
        }
        else {
            DebugConsole.LogError($"MapError: invalid map scene \"{map.ResourcePath}\"");
        }
=======
		map ??= MapScene;

		if (IsInstanceValid(map)) {

			DisposeMap();

			Map = map.Instantiate();

			if (Map is not null && IsInstanceValid(Map)) {
				GD.Print($"Set Map to {Map}");

				AddChild(Map);

				EmitSignalMapChanged();
			}
			else {
				DebugConsole.LogError($"MapError: failed to instantiate map scene \"{map.ResourcePath}\"");
			}
		}
		else {
			DebugConsole.LogError($"MapError: invalid map scene \"{map.ResourcePath}\"");
		}
>>>>>>> guh

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
			DebugConsole.LogError($"MapError: cannot find or failed to load scene \"{SceneDir}/{scene}.tscn\"");
			return null;
		}
	}

	public MapController LoadAndSetFromScene(string scene, bool useCache = true)
	{
		if (LoadFromScene(scene, useCache) is PackedScene packed) {
			MapScene = packed;
		}
		return this;
	}

	public MapController SetFromScene(PackedScene scene)
	{
		MapScene = scene;
		return this;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		DisposeMap();
   	}

	public override async void _Ready()
	{
		base._Ready();
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
<<<<<<< HEAD
        ResetApply();
	}
}
=======
		ResetApply();
	}
}
>>>>>>> guh
