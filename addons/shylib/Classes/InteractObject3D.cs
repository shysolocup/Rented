using Godot;
using System;

[GlobalClass, Icon("res://addons/shylib/Images/InteractObject3D.png")]
public partial class InteractObject3D : CollisionShape3D
{

	[Export] public bool Enabled = true;
	[Export] public StandardMaterial3D HoverIcon;

	private bool _pressed = false;
	private bool _hovering = false;


	public virtual void _Press() {}
	public virtual void _Hover() {}
	
	private Crosshair3D Crosshair; 

	public override void _Ready()
	{
		Crosshair = GetNode<Crosshair3D>("%Crosshair");
	}


	public override void _Input(InputEvent @event) 
	{
		if (Input.IsMouseButtonPressed(MouseButton.Left) && Hovering) Pressed = true;
		else Pressed = false;
	}



	[Export] public bool Pressed {
		get {
			return _pressed;
		}

		set {
			if (Enabled) _pressed = value;
			_Press();
		}
	}

	[Export] public bool Hovering {
		get {
			return _hovering;
		}

		set {
			if (Enabled) _hovering = value;
			_Hover();

			if (_hovering && Enabled) {
				Crosshair.Icon.SetSurfaceOverrideMaterial(0, HoverIcon);
			}
			else if (_hovering && !Enabled) {
				Crosshair.Icon.SetSurfaceOverrideMaterial(0, Crosshair.LockIcon);
			}
			else {
				Crosshair.Icon.SetSurfaceOverrideMaterial(0, Crosshair.DefaultIcon);
			}
		}
	}
}
