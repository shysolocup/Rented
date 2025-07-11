using Godot;

public partial class Item : Node
{
    [Export] public bool InInventory = false;
    [Export] public bool Stackable = false;

    [Signal] public delegate void EquippedEventHandler();
    [Signal] public delegate void UnequippedEventHandler();
    [Signal] public delegate void UsedEventHandler();

    public virtual void _Equipped() { }
    public virtual void _Unequipped() { }
    public virtual void _Used() { }
}

