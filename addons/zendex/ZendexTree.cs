using Godot;
using Godot.Collections;

public partial class ZendexTree : Label
{
    static string Treeicize(Array<NodePath> paths)
    {
        return "guh";
    }

    private Array<NodePath> _paths = [];

    [Export] public Array<NodePath> Paths {
        get => _paths;
        set {
            _paths = value;
            Text = Treeicize(_paths);
        }
    }

    public override void _Ready()
    {
        base._Ready();
    }
}