using Godot;
using System;

[Tool]
[GlobalClass, Icon("uid://dme3m2uv5jaju")]
public partial class RbxScript : Node
{
	[Signal] public delegate void EnabledChangedEventHandler(bool oldValue, bool newValue);
	[Signal] public delegate void SourceChangedEventHandler(bool oldValue, bool newValue);

	[ExportToolButton("Execute")] public Callable ExecuteCall => Callable.From(_execute);
	private void _execute() { Execute(); }

	public Variant Execute(params object[] args) {
		return ((Script)ReloadSource()).CallDeferred("_Ready");
	}

	[ExportToolButton("Reload Source")] public Callable ReloadSourceCall => Callable.From(ReloadSource);

	public Variant ReloadSource()
	{
		return GD.Load(Source);
	}

	private bool _enabled = true;

	[Export] public bool Enabled {
		get { return _enabled; }
		set { 
			var old = _enabled;
			_enabled = value; 
			if (old != value) EmitSignal(SignalName.EnabledChanged, old, value); 
		}
	}

	public string _source { get; set; }

	[Export(PropertyHint.File, "*.cs,.gd")] public string Source {
		get { return _source; }
		set {
			var old = _source;
			_source = value;
			if (old != value) EmitSignal(SignalName.SourceChanged, old, value); 
		}
	}

	[Export] public Node SpawnedNode;
	private Variant nullvar = new Variant();

	[Export] public NodePath SpawnedNodePath;


	public RbxScript DetatchScript()
	{
		GetParent().RemoveChild(SpawnedNode);
		SpawnedNode.QueueFree();
		SpawnedNode = null;
		return this;
	}


	public Node AttachScript(string source = null)
	{
		source ??= Source;

		if (SpawnedNode != null) {
			DetatchScript();
		}

		Variant Script = ReloadSource();

		if (Script.Obj is CSharpScript cs) SpawnedNode = (Node)cs.New();
		else if (Script.Obj is GDScript gd) SpawnedNode = (Node)gd.New();

		SpawnedNodePath = new NodePath($"{Name}Source");
		SpawnedNode.Name = SpawnedNodePath.ToString();
		
		GetParent().CallDeferred("add_child", SpawnedNode);

		return SpawnedNode;
	}


	private void HandleEnabledChange(bool oldVal, bool newVal)
	{
		if (newVal == true) {
			AttachScript();
		}
		else {
			if (SpawnedNode != null) {
				DetatchScript();
			}
		}
	}
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		if (!Engine.IsEditorHint()) {
			EnabledChanged += HandleEnabledChange;
			SpawnedNodePath = new NodePath($"{Name}Source");

			Set("SpawnedNode", nullvar);

			if (Source != null && Enabled) {
				AttachScript();
			};
		}
	}
}
