using Godot;


[Tool]
[GlobalClass]
public partial class DCEntry : EditorPlugin
{


	public static Variant Gamenode;
	public Variant Dock;

	public override void _EnterTree()
	{
		SceneChanged += (Node new_root) =>
		{	if (new_root != null)
			{
				Gamenode = new_root;
			}
		};


		// Add autoloads
		AddAutoloadSingleton("debug_console", "res://addons/copper_dc/debug_console.tscn");
	}

	public override void _ExitTree()
	{

		// Remove autoloads
		RemoveAutoloadSingleton("debug_console");
	}


}