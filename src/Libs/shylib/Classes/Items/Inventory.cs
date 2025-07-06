using System.Linq;
using Godot;
using Godot.Collections;

[Tool]
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
        base._Ready();

        if (Engine.IsEditorHint()) return;

        player = this.GetGameNode<Player>("%Player");
    }

    [ExportToolButton("Unequip")] private Callable ueToolButton => Callable.From(UnEquip);


    public void Collect(string itemName) => GetNode<Item>(itemName).InInventory = true;
    public void Throw(string itemName) => GetNode<Item>(itemName).InInventory = false;

    static public void UnEquip() => Equipped = null;

    public Item Equip(string itemName)
    {
        var node = GetNode<Item>(itemName);
        Equipped = node;
        return node;
    }

    public Array<Item> GetAllItems() => [.. GetChildren().Cast<Item>()];
    public Array<Item> GetOtherItems() => [.. GetChildren().Cast<Item>().Where(v => !v.Stackable && !v.InInventory)];
    public Array<Item> GetInventoryItems() => [.. GetChildren().Cast<Item>().Where(v => v.InInventory)];
    
    public override void _UnhandledInput(InputEvent @event)
    {
        base._Input(@event);

        if (!Engine.IsEditorHint() && Input.IsActionJustPressed("Use") && Equipped is not null)
        {
            Equipped._Used();
            Equipped.EmitSignal(Item.SignalName.Used);
        }
    }
}