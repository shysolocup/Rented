using Godot;
using System;

[GlobalClass, Icon("res://Script.png")]
public partial class RbxScript : Node
{
	private NodePath loadedName = new NodePath("LoadedScript");

	[Signal] public delegate void EnabledChangedEventHandler(bool oldValue, bool newValue);
	[Signal] public delegate void SourceChangedEventHandler(bool oldValue, bool newValue);

	public bool _enabled = true;

	[Export] public bool Enabled {
		get { return _enabled; }
		set { 
			var old = _enabled;
			_enabled = value; 
			if (old != value) EmitSignal(SignalName.EnabledChanged, old, value); 
		}
	}

	public CSharpScript _source { get; set; }

	[Export] public CSharpScript Source {
		get { return _source; }
		set {
			var old = _source;
			_source = value;
			if (old != value) EmitSignal(SignalName.SourceChanged, old, value); 
		}
	}

	private Node Loaded;
	private Variant nullvar = new Variant();


	public Node AttachScript(CSharpScript source)
	{
		var oldinstance = GetNodeOrNull(loadedName);
		if (oldinstance != null) RemoveChild(oldinstance);

		Loaded = (Node)source.New();
		Loaded.Name = loadedName.ToString();
		
		AddChild(Loaded);

		return Loaded;
	}


	private void HandleEnabledChange(bool oldVal, bool newVal)
	{
		if (newVal == true) {
			AttachScript(Source);
		}
		else {
			var instance = GetNodeOrNull(loadedName);
			if (instance != null) RemoveChild(instance);
		}
	}
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		EnabledChanged += HandleEnabledChange;

		Set("Loaded", nullvar);

		if (Source != null && Enabled) {
			AttachScript(Source);
		};
	}
}