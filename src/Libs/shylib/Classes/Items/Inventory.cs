using System.Linq;
using Godot;
using Godot.Collections;


[GlobalClass, Icon("uid://btrp46wvefnnq")]
public partial class Inventory : Node
{
    private Player player;

    static private Item equipped;

    static public Item Equipped
    {
        get => equipped;
        set
        {
            if (equipped is not null)
            {
                equipped._Unequipped();
                equipped.EmitSignal(Item.SignalName.Unequipped);
            }

            equipped = value;

            if (equipped is not null)
            {
                equipped._Equipped();
                equipped.EmitSignal(Item.SignalName.Equipped);   
            }
        }
    }

    [Export]
    private Node Equiped
    {
        get => Equipped;
        set => Equipped = value as Item;
    }

    public override void _Ready()
    {
        player = this.GetGameNode<Player>("%Player");
        base._Ready();
    }

    public Array<Item> GetItems()
    {
        return [.. GetChildren().Cast<Item>()];
    }
    
    public override void _UnhandledInput(InputEvent @event)
	{
		base._Input(@event);

        if (Input.IsActionJustPressed("Use") && Equipped is not null)
        {
            Equipped._Used();
            Equipped.EmitSignal(Item.SignalName.Used);
		}
	}
}