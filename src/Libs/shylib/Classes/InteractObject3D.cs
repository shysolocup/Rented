using Godot;
using System;

[GlobalClass, Icon("res://src/Libs/shylib/Images/InteractObject3D.png")]
public partial class InteractObject3D : RigidBody3D
{

	[Export] public bool Enabled = true;
	[Export] public StandardMaterial3D HoverIcon;
	[Export] public string Line = "interact_default";
	public bool Cooldown = false;
	private Player Player;

	private bool _pressed = false;
	private bool _hovering = false;


	[Signal] public delegate void PressEventHandler();
	[Signal] public delegate void HoverEventHandler();


	public virtual void _Press() {}
	public virtual void _Hover() {}
	
	private Crosshair3D Crosshair; 

	public override void _Ready()
	{
		Crosshair = GetNode<Crosshair3D>("%Crosshair");
		Player = GetNode<Player>("%Player");
	}


	public override void _Input(InputEvent @event) 
	{
		if (Input.IsActionJustPressed("Interact") && Hovering && !Player.InDialog && !Cooldown) Pressed = true;
		else Pressed = false;
	}



	[Export] public bool Pressed {
		get {
			return _pressed;
		}

		set {
			if (value != _pressed) {
				_pressed = (!Enabled || Cooldown) ? false : value;
				_Press();
				EmitSignal(SignalName.Press);
			}
		}
	}

	[Export] public bool Hovering {
		get {
			return _hovering;
		}

		set {
			if (value != _hovering) {
				_hovering = Cooldown ? false : value;

				if (_hovering && Enabled) {
					Crosshair.Icon.SetSurfaceOverrideMaterial(0, (HoverIcon != null) ? HoverIcon : Crosshair.DefaultHoverIcon);
				}
				else if (_hovering && !Enabled) {
					Crosshair.Icon.SetSurfaceOverrideMaterial(0, Crosshair.LockedIcon);
				}
				else if (!_hovering) {
					Crosshair.Icon.SetSurfaceOverrideMaterial(0, Crosshair.DefaultIcon);
				}

				_Hover();
				EmitSignal(SignalName.Hover);
			}
		}
	}
}
