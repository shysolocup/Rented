using Godot;
using System;

[GlobalClass, Icon("res://Script.png")]
public partial class RbxScript : Node
{
	[Signal] public delegate void EnabledChangedEventHandler(bool oldValue, bool newValue);
	[Signal] public delegate void SourceChangedEventHandler(bool oldValue, bool newValue);

	public bool _enabled = true;

	[Export] public bool Enabled {
		get { return _enabled; }
		set { 
			var old = _enabled;
			_enabled = value; 
			EmitSignal(SignalName.EnabledChanged, old, value); 
		}
	}

	public CSharpScript _source;

	[Export] public CSharpScript Source {
		get { return _source; }
		set {
			var old = _source;
			_source = value; 
			EmitSignal(SignalName.SourceChanged, old, value); 
		}
	}

	[Export] public Node Loaded;
	private Variant nullvar = new Variant();


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		Set("Loaded", nullvar);

		if (Source != null && Enabled) {
			var script = GD.Load<CSharpScript>(Source.ResourcePath);

			if (script == null) {
				GD.PrintErr("Failed to load script!");
			}

			Loaded = (Node)script.New();
			
			AddChild(Loaded);
			
			Loaded.Call("_Ready");
		};
	}

	public override void _Process(double delta)
	{
		if (Loaded != null && Enabled) {
			Loaded.Call("_Process", delta);
		}
	}
}
