using System.Threading.Tasks;
using Godot;

[GlobalClass, Icon("uid://dwuqrvw0jot8e")]
public partial class TalkerObject3D : InteractObject3D
{
	[Export] public string Line = "interact_default";
	private Player Player;

	public override void _Ready() {
		base._Ready();
		Player = this.GetGameNode<Player>("%Player");
		AutoCooldown = false;
	}

	public override void _HoverChanged()
	{
		if (Hovering) {
			GD.Print("Started hovering");
		}
		else {
			GD.Print("Stopped hovering");
		}
		base._HoverChanged();
	}
	
	public override void _PressChanged()
	{
		if (Pressed) {
			GD.Print("a");
			_ = Player.SnatchInteract(this);
		}

		base._PressChanged();
	}
}
