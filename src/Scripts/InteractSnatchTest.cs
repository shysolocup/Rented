using Godot;
using System;

public partial class InteractSnatchTest : Node
{
	public InteractObject3D obj;
	public Player player;

	public override void _Ready() {
		obj = GetParent<InteractObject3D>();
		player = GetNode<Player>("../%Player");

		obj.Hover += Hover;
		obj.Press += Press;
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
}
