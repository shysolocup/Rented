using Godot;
using System;

[GlobalClass, /*Icon("res://addons/shylib/Images/InteractObject.png")*/]
public partial class InteractObject3D : CollisionShape3D
{

    [Export] public bool Enabled = true;
    [Export] public Texture2D HoverIcon;

    private bool _pressed = false;
    private bool _hovering = false;


    public virtual void _Press() {}
    public virtual void _Hover() {}
    
    private Crosshair Icon; 

    public override async void _Ready()
    {
        Icon = await this.GetNodeAsync<Crosshair>("Crosshair");
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
                var material = Icon.GetActiveMaterial(0) as StandardMaterial3D;
                material.AlbedoTexture = HoverIcon;
            }
            else {
                var material = Icon.GetActiveMaterial(0) as StandardMaterial3D;
                material.AlbedoTexture = Icon.DefaultTexture;
            }
        }
    }
}