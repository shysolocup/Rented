using Godot;
using System;


[GlobalClass, Icon("res://addons/shylib/Images/Script.png")]
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

	[Export] private Node SpawnedNode;
	private Variant nullvar = new Variant();

	[Export] public NodePath SpawnedNodePath;


	public Node AttachScript(CSharpScript source)
	{
		if (SpawnedNode != null) {
			GetParent().RemoveChild(SpawnedNode);
			SpawnedNode = null;
		}

		SpawnedNode = (Node)source.New(this);

		SpawnedNodePath = new NodePath($"{Name}Source");
		SpawnedNode.Name = SpawnedNodePath.ToString();
		
		GetParent().CallDeferred("add_child", SpawnedNode);

		return SpawnedNode;
	}


	private void HandleEnabledChange(bool oldVal, bool newVal)
	{
		if (newVal == true) {
			AttachScript(Source);
		}
		else {
			if (SpawnedNode != null) {
				GetParent().RemoveChild(SpawnedNode);
				SpawnedNode = null;
			}
		}
	}
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		EnabledChanged += HandleEnabledChange;
		SpawnedNodePath = new NodePath($"{Name}Source");

		Set("SpawnedNode", nullvar);

		if (Source != null && Enabled) {
			AttachScript(Source);
		};
	}
}
