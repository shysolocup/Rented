using Godot;
using System;

public partial class ZendexUi : Control
{
	private LineEdit Search;
	private MenuButton AddLayer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var Top = GetChild(0).GetChild(0);
		var Bottom = GetChild(0).GetChild(1);

		Search = Top.GetChild<LineEdit>(1);
		AddLayer = Top.GetChild<MenuButton>(2);

		GD.Print(AddLayer);
		GD.Print(Search);

		Search.TextChanged += TextChanged;
	}

	private void TextChanged(string text) {
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
