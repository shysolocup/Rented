using CoolGame;
using Godot;
using System;
using System.Threading.Tasks;
using Godot.Collections;

[Tool]
[GlobalClass, Icon("uid://boo8iw5pvoaa8")]
public partial class GameInstance : Node
{

	/// <summary>
	/// current lighting environment
	/// </summary>
	/*[Export] public Godot.Environment GameEnvironment {
		get {
			return GetNode<WorldEnvironment>("%World").Environment;
		}
		set {
			if (value != null && value != GetNode<WorldEnvironment>("%World").Environment) {
				GetNode<WorldEnvironment>("%World").Environment = value;
			}
		}
	}

	[Export] public Godot.Environment DefaultGameEnvironment { get; set; }*/

	[ExportToolButton("Reset VFX")]
	public Callable ResetVFX => Callable.From(() =>
	{
		Lighting ??= GetNode<Lighting3D>("%Lighting3D");
		Map ??= GetNode<MapController>("%Map");
		Lighting.ResetApply();
		Map.ResetApply();
	});

	public Lighting3D Lighting;
	public MapController Map;

	public Player Player;


	public string Version = "0.0.1";
	public string VersionDenot = "ResBuilding";
	
	/// <summary>
	/// current noise level
	/// </summary>
	[Export] public float Noise = 0;

	/// <summary>
	/// the highest level of noise the player has made
	/// </summary>
	[Export] public float HighestNoise = 0;


	public void NoiseCalc(float delta)
	{
		float n = 0;
		n += (Player.Velocity.Length() * 20 - n) * this.FactorDelta(1/5f, delta);

		Noise += (n - Noise) * this.FactorDelta(1/10f, delta);

		GD.Print(Noise);

		if (Noise > HighestNoise)
		{
			HighestNoise = Noise;
		}

		Task.Delay(10);
	}

	/// <summary>
	/// Game gravity what more do you want me to say
	/// </summary>
	public float Gravity
	{
		get => (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
		set => ProjectSettings.SetSetting("physics/3d/default_gravity", value);
	}

	/// <summary>
	/// Fires if the game is currently saving
	/// </summary>
	[Signal] public delegate void SavingEventHandler();

	/// <summary>
	/// Fires when the game finishes saving
	/// </summary>
	[Signal] public delegate void SavedEventHandler();

	public Variant nullvar = new Variant();

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest)
		{
			Game.Save();
			GetTree().Quit();
		}
	}

	public Node LoadRoom(string room)
	{
		return LoadRoom<Node>(room);
	}

	public T LoadRoom<T>(string room) where T : Node
	{
		T scn = GD.Load<PackedScene>($"res://src/Scenes/Rooms/{room}.tscn").Instantiate<T>();
		GetNode<Node>("%Rooms").AddChild(scn);
		return scn;
	}

	/*
		
	*/

	public void IterOverwrite(Variant thing, Variant filedata, Variant basedata)
	{
	}

	private void Iter(Variant thing, Variant filedata, Variant basedata)
	{
		if (thing.GetType() == typeof(Godot.Collections.Array))
		{
			Array<Variant> data = (Array<Variant>)thing;
			for (int i = 0; i < data.Count; i++)
			{
				Iter(data[i], data, ((object)basedata is not null && basedata.GetType() == data.GetType()) ? ((Godot.Collections.Array)basedata)[i] : nullvar);
			}
		}
		else if (thing.GetType() == typeof(Dictionary))
		{
			foreach ((string key, Variant value) in (Dictionary<string, Variant>)thing)
			{

			}
		}
		else
		{

		}
	}

	public Dictionary<string, Variant> UseTemplate(bool exists = false)
	{
		string source = Json.Stringify(Game.SaveTemplate, "\t");
		using var writer = FileAccess.Open(Game.SavePath, FileAccess.ModeFlags.Write);

		var json = new Json();
		json.Parse(source);

		var data = (Dictionary<string, Variant>)json.Data;
		var basefile = (Dictionary<string, Variant>)data["file_base"];

		for (int i = 0; i < 3; i++)
		{
			string file = $"file_{i}";
			if (exists && !(bool)basefile["overwrite"] && (object)data[file] is not null)
			{
				Dictionary<string, Variant> filedata = (Dictionary<string, Variant>)data[file];

				foreach ((string key, Variant value) in basefile)
				{
					Iter(value, filedata, basefile);
				}
			}
			else if (exists && (bool)basefile["overwrite"])
			{
				foreach ((string key, Variant value) in basefile)
				{
					#region do smth with this
					// this part is here if the save should overwrite or not
					#endregion
				}
			}

			data[file] = basefile;
		}

		data.Remove("file_base");
		data.Remove("overwrite");

		writer.StoreString(Json.Stringify(data, "\t"));

		foreach (var (key, value) in data)
		{
			Game.Saves[key] = value;
		}

		return data;
	}

	/// <summary>
	/// Waits for a given time
	/// </summary>
	/// <param name="time">time in milliseconds to wait for</param>
	public async void Sleep(float time)
	{
		SceneTreeTimer t = GetTree().CreateTimer(time);
		await ToSignal(t, SceneTreeTimer.SignalName.Timeout);
		t.Dispose();
	}


	/// <summary>
	/// Waits for a given time
	/// </summary>
	/// <param name="time">time in milliseconds to wait for</param>
	public async void Delay(float time, Func<Task> callback)
	{
		SceneTreeTimer t = GetTree().CreateTimer(time);
		await ToSignal(t, SceneTreeTimer.SignalName.Timeout);
		t.Dispose();

		await callback();
	}


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Game.Instance = this;
		Lighting = GetNode<Lighting3D>("%Lighting3D");
		Map = GetNode<MapController>("%Map");
		Player = GetNode<Player>("%Player");

		if (Engine.IsEditorHint()) return;

		Game.SaveTemplate = Game.ReadJson("res://src/Data/SaveTemplate.json");

		/*GameEnvironment = (DefaultGameEnvironment != null) ? DefaultGameEnvironment : GameEnvironment;
		DefaultGameEnvironment = GameEnvironment;*/

		if (!FileAccess.FileExists(Game.SavePath))
		{
			UseTemplate();
			return;
		}

		using var savedata = FileAccess.Open(Game.SavePath, FileAccess.ModeFlags.Read);

		{
			string textcheck = savedata.GetAsText();
			string[] toBeRemoved = [" ", "{", "}", "\n", "\t"];

			foreach (string remover in toBeRemoved)
			{
				textcheck = textcheck.Replace(remover, "");
			}

			if (textcheck == "")
			{
				UseTemplate();
				return;
			}
		}

		var json = new Json();
		var res = json.Parse(Json.Stringify(Json.ParseString(savedata.GetAsText()), "\t"));

		var data = (Dictionary<string, Variant>)json.Data;

		if ((string)data["version"] != (string)Game.SaveTemplate["version"])
		{
			UseTemplate(true);
			return;
		}

		data.Remove("file_base");

		foreach (var (key, value) in data)
		{
			Game.Saves[key] = value;
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (Engine.IsEditorHint()) return;

		NoiseCalc((float)delta);
	}

}
