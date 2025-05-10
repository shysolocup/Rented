using System.Threading.Tasks;
using Godot;

[GlobalClass, Icon("uid://b2fdc8cj70qou")]
public partial class ItemObject3D : InteractObject3D
{

    private Player Player;

    public override void _Ready() {
        base._Ready();
    }
    
    public override void _PressChanged()
    {
        base._PressChanged();
    }
}

/*
public InteractObject3D obj;
	public Player player;

	public override void _Ready() {
		obj = GetParent<InteractObject3D>();
		player = this.GetGameNode<Player>("%Player");

		obj.HoverChanged += Hover;
		obj.PressChanged += Press;
	}

	public void Hover()
	{
		if (obj.Hovering) {
			GD.Print("Started hovering");
		}
		else {
			GD.Print("Stopped hovering");
		}
	}

	public async void Press()
	{
		if (obj.Pressed) {
			await player.SnatchInteract(obj);
		}
	}
    */