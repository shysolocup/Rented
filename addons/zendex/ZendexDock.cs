using System.Linq;
using Godot;
using Godot.Collections;
using Appox;
using System.Collections.Generic;
using System;

[Tool]
[GlobalClass]
public partial class ZendexDock : Control
{
	[Export] public Array<ZendexLayer> Layers = [];

	private LineEdit SearchLabel;
	private Button AddLayerButton;
	private EditorSelection Selection = EditorInterface.Singleton.GetSelection();

	private Popup AddLayerPopup;

	static private VBoxContainer LayersContainer;

	static private ZendexLayerNode BaseLayer;
	

	public override void _Ready()
	{
		var Top = GetChild(0).GetChild(0);
		var Bottom = GetChild(0).GetChild(1);

		SearchLabel = Top.GetChild<LineEdit>(1);
		AddLayerButton = Top.GetChild<Button>(2);

		BaseLayer = Bottom.GetChild<ZendexLayerNode>(0);
		LayersContainer = Bottom.GetChild(1).GetChild<VBoxContainer>(0);

		AddLayerPopup = AddLayerButton.GetChild<Popup>(0);

		Selection.SelectionChanged += SelectionChanged;

		GD.Print(AddLayerButton);
		GD.Print(SearchLabel);

		SearchLabel.TextChanged += SearchChanged;
		AddLayerButton.Pressed += PromptAddLayer;

		foreach ( CanvasItem node in GetTree().Root.GetDescendants<CanvasItem>()) {
			if (Layers.ElementAtOrDefault(node.ZIndex) is ZendexLayer layer) {
				Layers[node.ZIndex].Children.Add(node);
			}
			else {
				AddLayer("Unknown", node.ZIndex);
			}
		}

		AddLayer("Default", 0);
	}

    public void SelectionChanged()
	{
		Array<CanvasItem> items = [];
		Array<Node> selected = Selection.GetSelectedNodes();

		foreach ( (Node item, int index) in ShyUtil.Enumerate(selected)) {
			if (item is CanvasItem) items.Add((CanvasItem)item);

			if (index + 1 == selected.Count && items.Count == 0) {
				AddLayerButton.Text = "➕ ";
				return;
			}
		}

		GD.Print(items);
		AddLayerButton.Text = (items.Count > 0) ? $"➕ {items.Count} Nodes to new layer" : "➕ ";
	}

	static public ZendexLayer AddLayer(string title, int index, Array<CanvasItem> children = null)
	{
		var layer = new ZendexLayer(title, index, children);
		var clone = BaseLayer.Duplicate<ZendexLayerNode>();
		layer.LayerNode = clone;

		LayersContainer.AddChild(clone);

		return layer;
	}

	private void PromptAddLayer()
	{
		GD.Print(Selection.GetSelectedNodes());
		AddLayerPopup.Show();
	}

	private void SearchChanged(string text) {
		
	}

	public override void _Process(double delta)
	{
		
	}
}
