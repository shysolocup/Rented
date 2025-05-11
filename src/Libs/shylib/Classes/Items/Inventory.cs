using System.Linq;
using Godot;
using Godot.Collections;

[Tool]
[GlobalClass, Icon("uid://btrp46wvefnnq")]
public partial class Inventory : Node 
{
    private Player player;

    public override void _Ready()
    {
        player = this.GetGameNode<Player>("%Player");
        base._Ready();
    }

    public Array<Item> GetItems() 
    {
        return [.. GetChildren().Cast<Item>()];
    }
}