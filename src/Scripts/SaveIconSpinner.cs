using Godot;
using System;
using CoolGame;

public partial class SaveIconSpinner : Sprite2D
{
	// Called when the node enters the scene tree for the first time.

	public override async void _Ready()
	{
		await Game.Init();
		GD.Print(Game.Saves);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
