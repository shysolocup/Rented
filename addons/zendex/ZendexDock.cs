using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class ZendexDock : Control
{
	[Export] public Array<ZendexLayer> Layers = [];

	private LineEdit Search;
	private MenuButton AddLayer;
	private EditorSelection Selection;
	

	public override void _Ready()
	{
		var Top = GetChild(0).GetChild(0);
		var Bottom = GetChild(0).GetChild(1);

		Search = Top.GetChild<LineEdit>(1);
		AddLayer = Top.GetChild<MenuButton>(2);

		GD.Print(AddLayer);
		GD.Print(Search);

		Search.TextChanged += SearchChanged;
	}

	public void SelectionChanged(Array<Node> selected)
	{
		GD.Print(selected);
		AddLayer.Text = $"➕ {selected.Count} Nodes";
	}

	private void SearchChanged(string text) {
		
	}

	public override void _Process(double delta)
	{
		
	}
}
