using Godot;

[GlobalClass, Icon("uid://b2fdc8cj70qou")]
public partial class ItemObject3D : InteractObject3D
{
	[Export] public StandardMaterial3D boog = GD.Load<StandardMaterial3D>("uid://ivdjd8ibadn6");
    private Player Player;

    public override void _Ready() {
        base._Ready();
		HoverIcon = boog;
    }
    
    public override void _PressChanged()
    {
        base._PressChanged();
		Enabled = false;
		Hide();
    }
}