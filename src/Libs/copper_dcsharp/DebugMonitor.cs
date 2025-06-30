using Godot;


public partial class DebugMonitor : GodotObject
	{

		public string Id;
		public string DisplayName;
		public Variant Value;
		public bool Visible = OS.HasFeature("debug");
		public Callable ValueCall;

		public void Update()
		{
			Value = ValueCall.Call();
		}

		public void AddTo(DebugConsole console) 
		{
			console.Monitors.Add(Id, this);
		}
	}
