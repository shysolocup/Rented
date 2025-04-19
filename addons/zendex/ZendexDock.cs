using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class ZendexDock : Control
{
	private LineEdit Search;
	private MenuButton AddLayer;
	private EditorSelection Selection;

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

	public void SelectionChanged(Array<Node> selected)
	{
		GD.Print(selected);
	}

	private void TextChanged(string text) {
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
