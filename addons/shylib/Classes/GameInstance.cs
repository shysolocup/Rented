using CoolGame;
using Godot;
using System;
using System.Threading.Tasks;
using Godot.Collections;



namespace CoolGame
{

	/// <summary>
	/// Global class for holding game data and nodes
	/// </summary>
	public static class Game
	{

		/// <summary>
		/// Node responsible for holding the game data
		/// </summary>
		public static GameInstance Instance { get; set; }


		/// <summary>
		/// If the game is currently saving
		/// </summary>
		public static bool Saving {get; set; } = false;


		/// <summary>
		/// Path to save player data to
		/// </summary>
		public static string SavePath { get; set; }  = "user://savedata.json";
		

		/// <summary>
		/// Contains player save data
		/// </summary>
		public static Dictionary<string, Variant> Saves { get; set; } = new();


		/// <summary>
		/// Template for saves
		/// </summary>
		public static Dictionary<string, Variant> SaveTemplate { get; set; }


		/// <summary>
		/// Different states the player can be in
		/// </summary>
		public static Array<int> SaveStates { get; set; }


		/// <summary>
		/// Waits until Game.Instance exists
		/// </summary>
		/// <returns>Node</returns>
		public static async Task<Node> Init()
		{
			var waitTask = Task.Run(async () => {
				while (Instance == null || Saves == null) await Task.Delay(25);
			});

			if (waitTask != await Task.WhenAny(waitTask, Task.Delay(120000))) {
				throw new TimeoutException();
			}

			return Instance;
		}


		/// <summary>
		/// Reads json data and returns a dictionary
		/// </summary>
		/// <returns>Godot.Collections.Dictionary</returns>
		public static Dictionary<string, Variant> ReadJson(string fileDir, FileAccess.ModeFlags flag = FileAccess.ModeFlags.ReadWrite)
		{
			using var data = FileAccess.Open(fileDir, flag);

			var json = new Json();
			var res = json.Parse(data.GetAsText());

			var dict = (Dictionary<string, Variant>)json.Data;

			return dict;
		}

		/// <summary>
		/// Dictionary of events with string keys and StringName values
		/// </summary>
		private static readonly Dictionary<string, StringName> Events = new() {
			{"Saving", new StringName("Saving")},
			{"Saved", new StringName("Saved")},
		};


		/// <summary>
		/// Saves Game.SaveData to Game.SavePath
		/// </summary>
		/// <returns>Godot.Collections.Dictionary</returns>
		public static Dictionary<string, Variant> Save()
		{
			using var writer = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
			Saving = true;
			Instance.EmitSignal(Events["Saving"]);

			var data = ReadJson(SavePath);
			
			foreach (var (key, value) in Saves) {
				data[key] = value;
			}

			writer.StoreString(Json.Stringify(data, "\t"));
			Saving = false;
			Instance.EmitSignal(Events["Saved"]);

			return data;
		}
	}
}

[Tool]
[GlobalClass, Icon("res://addons/shylib/Images/Game.png")]
public partial class GameInstance : Node
{

	[Export] public string Version = "0.0.1";
	[Export] public string VersionDenot = "ResBuilding";
	/// <summary>
	/// current noise level
	/// </summary>
	[Export] public float Noise = 0;
	/// <summary>
	/// the highest level of noise the player has made
	/// </summary>
	[Export] public float HighestNoise = 100;
	/// <summary>
	/// Game gravity what more do you want me to say
	/// </summary>
	[Export] public float Gravity { 
			get {
				return (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
			} 
			set {
				ProjectSettings.SetSetting("physics/3d/default_gravity", value);
			} 
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
		if (what == NotificationWMCloseRequest) {
			Game.Save();
			GetTree().Quit();
		}
	}

	/*
		
	*/

	public void IterOverwrite(Variant thing, Variant filedata, Variant basedata) 
	{
	}

	private void Iter(Variant thing, Variant filedata, Variant basedata)
	{
		if (thing.GetType() == typeof(Godot.Collections.Array)) {
			Array<Variant> data = (Array<Variant>)thing;
			for (int i = 0; i < data.Count; i++) {
				Iter(data[i], data, ((object)basedata != null && basedata.GetType() == data.GetType()) ? ((Godot.Collections.Array)basedata)[i] : nullvar);
			}
		}
		else if (thing.GetType() == typeof(Dictionary)) {
			foreach ( (string key, Variant value) in (Dictionary<string, Variant>)thing) {
				
			}
		}
		else {

		}
	}

	public Dictionary<string, Variant> UseTemplate(bool exists = false)
	{
		string source = Json.Stringify(Game.SaveTemplate, "\t");
		using var writer = FileAccess.Open(Game.SavePath, FileAccess.ModeFlags.Write);

		var json = new Json();
		var res = json.Parse(source);

		var data = (Dictionary<string, Variant>)json.Data;
		var basefile = (Dictionary<string, Variant>)data["file_base"];

		for (int i = 0; i < 3; i++) {
			string file = $"file_{i}";
			if (exists && !(bool)basefile["overwrite"] && (object)data[file] != null) {
				Dictionary<string, Variant> filedata = (Dictionary<string, Variant>)data[file];
				
				foreach ( (string key, Variant value) in basefile) {
					Iter(value, filedata, basefile);
				}
			}
			else if (exists && (bool)basefile["overwrite"]) {
				foreach ( (string key, Variant value) in basefile) {

				}
			}

			data[file] = basefile;
		}

		data.Remove("file_base");
		data.Remove("overwrite");

		writer.StoreString(Json.Stringify(data, "\t"));
	
		foreach (var (key, value) in data) {
			Game.Saves[key] = value;
		}

		return data;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Game.Instance = this;

		Game.SaveTemplate = Game.ReadJson("res://src/Data/SaveTemplate.json");

		if (!FileAccess.FileExists(Game.SavePath)) {
			UseTemplate();
			return;
		}

		using var savedata = FileAccess.Open(Game.SavePath, FileAccess.ModeFlags.Read);

		{
			string textcheck = savedata.GetAsText();
			string[] toBeRemoved = { " ", "{", "}", "\n", "\t" };

			foreach (string remover in toBeRemoved) {
				textcheck = textcheck.Replace(remover, "");
			}
		
			if (textcheck == "") {
				UseTemplate();
				return;
			}
		}
		
		var json = new Json();
		var res = json.Parse(Json.Stringify(Json.ParseString(savedata.GetAsText()), "\t"));

		var data = (Dictionary<string, Variant>)json.Data;

		if ((string)data["version"] != (string)Game.SaveTemplate["version"]) {
			UseTemplate(true);
			return;
		}

		data.Remove("file_base");

		foreach (var (key, value) in data) {
			Game.Saves[key] = value;
		}
	}
}
