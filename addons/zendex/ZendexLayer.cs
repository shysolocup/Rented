using Godot;
using Godot.Collections;

public partial class ZendexLayer : Resource
{
	public Array<CanvasItem> Children = [];
	public int Index;
	public string Title;
    public ZendexLayerNode LayerNode;

    public ZendexLayer(string title, int index, Array<CanvasItem> children = null) : base()
    {
        Title = title;
        Index = index;
        Children = children;
    }
}
