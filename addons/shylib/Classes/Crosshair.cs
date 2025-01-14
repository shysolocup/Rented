using Godot;
using System;

[GlobalClass, /*Icon("res://addons/shylib/Images/Crosshair.png")*/]
public partial class Crosshair : MeshInstance3D
{
    [Export] public Texture2D DefaultTexture;
    [Export] public float Distance = 1;
    [Export] public float Weight = 0.2f;
    private Camera3D camera;

    public override async void _Ready()
    {
        camera = await this.GetNodeAsync<Camera3D>("PlayerCamera");
    }

    public override void _Process(double delta)
    {
        Vector3 forward = -camera.Transform.Basis.Z;
        Vector3 target = camera.GlobalTransform.Origin + forward * Distance;

        Transform3D transform = new Transform3D(GlobalTransform.Basis, camera.GlobalTransform.Origin * forward * Distance);

        GlobalTransform = GlobalTransform.InterpolateWith(transform, Weight * (float)delta);
    }
}