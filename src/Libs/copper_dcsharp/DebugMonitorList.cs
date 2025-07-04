using Godot;
using CoolGame;
using System.Linq;
using Appox;
using System;

public static class DebugMonitorList
{
	private static string fps()
	{
		var fps = (float)Mathf.Snapped(Performance.GetMonitor(Performance.Monitor.TimeFps), 0.001);

		float green = fps != Game.Settings.TargetFPS ? fps / Game.Settings.TargetFPS : 0;
		float red = fps != Game.Settings.TargetFPS ? 1 - (fps / Game.Settings.TargetFPS) : 0;

		Color col = new(red, green, fps == Game.Settings.TargetFPS ? 1 : 0);

		string value = $"[color={col.ToHtml()}]{fps}[/color]";
		return value;
	}
	private static float process() => (float)Mathf.Snapped(Performance.GetMonitor(Performance.Monitor.TimeProcess), 0.001);

	private static float physics_process() => (float)Mathf.Snapped(Performance.GetMonitor(Performance.Monitor.TimePhysicsProcess), 0.001);
	
	private static float nav_process() => (float)Mathf.Snapped(Performance.GetMonitor(Performance.Monitor.TimeNavigationProcess), 0.001);

	private static float static_memory() => (float)Mathf.Snapped(Performance.GetMonitor(Performance.Monitor.MemoryStatic), 0.001);

	private static float max_static_memory() => (float)Mathf.Snapped(Performance.GetMonitor(Performance.Monitor.MemoryStaticMax), 0.001);
	
	private static float objects() => (float)Performance.GetMonitor(Performance.Monitor.ObjectCount);

	private static float nodes() => (float)Performance.GetMonitor(Performance.Monitor.ObjectNodeCount);

	private static string noise()
	{
		string guh = $"\n    [val: {Game.Instance.Noise}]";
		foreach (var (k, v) in Game.Instance.NoiseGlob)
		{
			if (v == 0) continue;
			guh += $"\n    [{k}: {v}]";
		}
		return guh;
	}

	private static string pause()
	{
		return $"\n[IsPaused: {Game.Instance.GetGameNode<PauseGui>("%PauseLayer/PauseGui")?.Paused}]\n[InSettings: {Game.Instance.GetGameNode<SettingsGui>("%SettingsLayer/SettingsGui")?.InSettings}]";
	}


	public static async void Init(DebugConsole console)
	{
		await Game.Init();

		#region fps

		new DebugMonitor
		{
			Id = "fps",
			DisplayName = "FPS",

			ValueCall = Callable.From(fps)
		}.AddTo(console);


		#endregion
		#region process


		new DebugMonitor
		{
			Id = "process",
			DisplayName = "Process",

			ValueCall = Callable.From(process)
		}.AddTo(console);


		#endregion
		#region physics_process


		new DebugMonitor
		{
			Id = "physics_process",
			DisplayName = "Physics Process",

			ValueCall = Callable.From(physics_process)
		}.AddTo(console);


		#endregion
		#region navigation_process


		new DebugMonitor
		{
			Id = "navigation_process",
			DisplayName = "Navigation Process",

			ValueCall = Callable.From(nav_process)
		}.AddTo(console);


		#endregion
		#region static_memory


		new DebugMonitor
		{
			Id = "static_memory",
			DisplayName = "Static Memory",

			ValueCall = Callable.From(static_memory)
		}.AddTo(console);


		#endregion
		#region static_memory_max


		new DebugMonitor
		{
			Id = "static_memory_max",
			DisplayName = "Max Static Memory",

			ValueCall = Callable.From(max_static_memory)
		}.AddTo(console);


		#endregion
		#region objects


		new DebugMonitor
		{
			Id = "objects",
			DisplayName = "Objects",

			ValueCall = Callable.From(objects)
		}.AddTo(console);


		#endregion
		#region nodes


		new DebugMonitor
		{
			Id = "nodes",
			DisplayName = "Nodes",

			ValueCall = Callable.From(nodes)
		}.AddTo(console);


		#endregion
		#region noise


		new DebugMonitor
		{
			Id = "noise",
			DisplayName = "Noise",

			ValueCall = Callable.From(noise)
		}.AddTo(console);

		#endregion
		#region pause

		new DebugMonitor
		{
			Id = "pause",
			DisplayName = "PauseData",

			ValueCall = Callable.From(pause)
		}.AddTo(console);


		#endregion
	}
}
