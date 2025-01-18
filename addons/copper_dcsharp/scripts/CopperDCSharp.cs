using Godot;


[Tool]
[GlobalClass]
public partial class CopperDCSharp : EditorPlugin
{
	public Variant Dock;

	public override void _EnterTree()
	{
		// Add autoloads
		AddAutoloadSingleton("debug_console", "res://addons/copper_dcsharp/debug_console.tscn");
	}

	public override void _ExitTree()
	{
		// Remove autoloads
		RemoveAutoloadSingleton("debug_console");
	}


}