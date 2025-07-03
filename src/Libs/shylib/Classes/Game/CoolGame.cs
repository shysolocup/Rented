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
		public static bool Saving { get; set; } = false;


		/// <summary>
		/// Path to save player data to
		/// </summary>
		public static string SavePath { get; set; } = "user://savedata.json";


		/// <summary>
		/// Contains player save data
		/// </summary>
		public static Dictionary<string, Variant> Saves { get; set; } = [];

		/// <summary>
		/// 
		public static GameSettings Settings;

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
			var waitTask = Task.Run(async () =>
			{
				while (Instance == null || Saves == null) await Task.Delay(25);
			});

			if (waitTask != await Task.WhenAny(waitTask, Task.Delay(120000)))
			{
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
			var data = FileAccess.Open(fileDir, flag);

			var json = new Json();
			var res = json.Parse(data.GetAsText());

			var dict = (Dictionary<string, Variant>)json.Data;

			return dict;
		}

		/// <summary>
		/// Dictionary of events with string keys and StringName values
		/// </summary>
		private static readonly Dictionary<string, StringName> Events = new()
		{
			["Saving"] = new StringName("Saving"),
			["Saved"] = new StringName("Saved"),
		};


		/// <summary>
		/// Saves Game.SaveData to Game.SavePath
		/// </summary>
		/// <returns>Godot.Collections.Dictionary</returns>
		public static Dictionary<string, Variant> Save()
		{
			var writer = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
			Saving = true;
			Instance.EmitSignal(Events["Saving"]);

			var data = ReadJson(SavePath);

			foreach (var (key, value) in Saves)
			{
				data[key] = value;
			}

			writer.StoreString(Json.Stringify(data, "\t"));
			Saving = false;
			Instance.EmitSignal(Events["Saved"]);

			return data;
		}

		public static void Delay(float time, Action callback)
		{
			Instance.GetTree().CreateTimer(time).Connect(SceneTreeTimer.SignalName.Timeout, Callable.From(callback));
		}
	}
}