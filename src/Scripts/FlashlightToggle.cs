using Godot;
using System;
using System.Linq;

public partial class FlashlightToggle : MeshInstance3D
{
	private bool on = false;
	private Node3D lights;

	[Export]
	public float EnergyMult = 1;

	[Export]
	public bool On
	{
		get => on;
		set
		{
			on = value;
			lights.Visible = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();

		lights = GetChild<Node3D>(0);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (lights.Visible) {
			foreach (SpotLight3D light in lights.GetChildren().Cast<SpotLight3D>())
			{
				light.LightEnergy = this.Twlerp(light.LightEnergy, light.LightEnergy * EnergyMult, 1 / 1.5f, delta) - GD.Randf();
			}
		}
	}


	public override void _UnhandledInput(InputEvent @event)
	{
		base._Input(@event);

		if (Input.IsActionJustPressed("FlashlightToggle"))
		{
			On ^= true;
		}
	}

}
