using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class ZendexLayer : VBoxContainer
{
    [Export] public Array<Node> Children = [];

    public SpinBox Index;
    public LineEdit Title;
    public Button AddButton;
    public Button DeleteButton;

    public override void _Ready()
    {
        base._Ready();
        var container = GetChild(0);

        Index = container.GetChild<SpinBox>(0);
        Title = container.GetChild<LineEdit>(1);

        AddButton = container.GetChild<Button>(2);
        DeleteButton = container.GetChild<Button>(3);

        AddButton.Pressed += AddPressed;
        DeleteButton.Pressed += DeletePressed;
    }

    private void AddPressed()
    {
    }

    private void DeletePressed()
    {

    }
}