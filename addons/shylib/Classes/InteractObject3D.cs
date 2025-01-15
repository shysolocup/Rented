using Godot;
using System;

[GlobalClass, /*Icon("res://addons/shylib/Images/InteractObject.png")*/]
public partial class InteractObject3D : CollisionShape3D
{

	[Export] public bool Enabled = true;
	[Export] public StandardMaterial3D HoverIcon;

	private bool _pressed = false;
	private bool _hovering = false;


	public virtual void _Press() {}
	public virtual void _Hover() {}
	
	private Crosshair3D Crosshair; 

	public override async void _Ready()
	{
		Crosshair = await this.GetNodeAsync<Crosshair3D>("%Crosshair");
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

			if (_hovering) {
				Crosshair.Icon.SetSurfaceOverrideMaterial(0, HoverIcon);
			}
			else {
				Crosshair.Icon.SetSurfaceOverrideMaterial(0, Crosshair.DefaultIcon);
			}
		}
	}
}
