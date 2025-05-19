using Godot;
using System;
using System.Linq;

public partial class FlashlightToggle : MeshInstance3D
{
	private Node3D lights;

	[Export]
	public float EnergyMult = 1;
	public float Energy;
	public Flashlight flashlight;

	[Export]
	public bool On
	{
		get => !flashlight.Broken && flashlight.On;
		set
		{
			flashlight.On = value;
			lights.Visible = flashlight.On;
		}
	}

	public override void _Ready()
	{
		base._Ready();

		lights = GetChild<Node3D>(0);

		flashlight = this.GetGameNode<Flashlight>("%Inventory/Flashlight");

		flashlight.On = lights.Visible;
		Energy = lights.GetChild<Light3D>(0).LightEnergy;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (lights.Visible)
		{
			foreach (Light3D light in lights.GetChildren().Cast<Light3D>())
			{
				float guh = Energy * EnergyMult;

				light.LightEnergy = this.Twlerp(
					light.LightEnergy,
					Mathf.Clamp(light.LightEnergy - GD.RandRange(-1, 1), guh - 1, guh + 1),
					1/1.5f,
					delta
				);
			}
		}
	}


	public override void _UnhandledInput(InputEvent @event)
	{
		base._Input(@event);

		if (@event is InputEventKey action && action.IsActionPressed("FlashlightToggle"))
		{
			GD.Print(@event.GetType());
			On ^= true;
		}
	}

}
