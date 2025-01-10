using Godot;
using System;

public partial class rbxscriptest : RbxScript
{

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("a");
	}

	public override void _Process(double delta)
	{
		// GD.Print("b");
	}
}
