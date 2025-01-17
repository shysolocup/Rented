using System;
using System.Linq;
using System.Reflection;
using Godot;
using CoolGame;

public static class DebugMonitorList
{

	public static async void Init(DebugConsole console)
	{
        #region fps

		new DebugMonitor {
			Id = "fps",
			DisplayName = "FPS",

			ValueCall = Callable.From( () => {
                return (float)Performance.GetMonitor(Performance.Monitor.TimeFps);
            })
		}.AddTo(console);


		#endregion
		#region process


		new DebugMonitor {
			Id = "process",
			DisplayName = "Process",

			ValueCall = Callable.From( () => {
                return (float)Mathf.Snapped(Performance.GetMonitor(Performance.Monitor.TimeProcess), 0.001);
            })
		}.AddTo(console);


		#endregion
		#region physics_process


		new DebugMonitor {
			Id = "physics_process",
			DisplayName = "Physics Process",

			ValueCall = Callable.From( () => {
                return (float)Mathf.Snapped(Godot.Performance.GetMonitor(Godot.Performance.Monitor.TimePhysicsProcess), 0.001);
            })
		}.AddTo(console);


		#endregion
		#region navigation_process


		new DebugMonitor {
			Id = "navigation_process",
			DisplayName = "Navigation Process",

			ValueCall = Callable.From( () => {
                return (float)Mathf.Snapped(Godot.Performance.GetMonitor(Godot.Performance.Monitor.TimeNavigationProcess), 0.001);
            })
		}.AddTo(console);


		#endregion
		#region static_memory


		new DebugMonitor {
			Id = "static_memory",
			DisplayName = "Static Memory",

			ValueCall = Callable.From( () => {
                return (float)Mathf.Snapped(Performance.GetMonitor(Performance.Monitor.MemoryStatic), 0.001);
            })
		}.AddTo(console);


		#endregion
		#region static_memory_max


		new DebugMonitor {
			Id = "static_memory_max",
			DisplayName = "Max Static Memory",

			ValueCall = Callable.From( () => {
                return (float)Mathf.Snapped(Performance.GetMonitor(Performance.Monitor.MemoryStaticMax), 0.001);
            })
		}.AddTo(console);

		
		#endregion
		#region objects


		new DebugMonitor {
			Id = "objects",
			DisplayName = "Objects",

			ValueCall = Callable.From( () => {
                return (float)Performance.GetMonitor(Performance.Monitor.ObjectCount);
            })
		}.AddTo(console);


		#endregion
		#region nodes


		new DebugMonitor {
			Id = "nodes",
			DisplayName = "Nodes",

			ValueCall = Callable.From( () => {
                return (float)Performance.GetMonitor(Performance.Monitor.ObjectNodeCount);
            })
		}.AddTo(console);


        #endregion
        #region noise


        await Game.Init();


        new DebugMonitor {
			Id = "noise",
			DisplayName = "Noise",

			ValueCall = Callable.From( () => {
                return Game.Instance.Noise;
            })
		}.AddTo(console);


        #endregion
	}
}