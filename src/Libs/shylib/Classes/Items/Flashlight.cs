using Godot;
using System.Linq;


[Tool]
[GlobalClass, Icon("uid://b4blw6r8ym2hk")]
public partial class Flashlight : Item
{
	static private bool broken = false;
	static private bool on = false;

	[Export]
	public float EnergyMult = 1;
	public float Energy;

	static public MeshInstance3D Model;
	static public Node3D Lights;

	[Export]
	public bool On
	{
		get => !broken && on;
		set
		{
			on = value;
			Lights.Visible = On;
		}
	}

	// will play an animation for the flashlight breaking and fixing
	[Export]
	public bool Broken
	{
		get => broken;
		set
		{
			if (value) On = false;
			broken = value;
		}
	}

	public override void _Equipped()
	{
		base._Equipped();
	}

	public override void _Unequipped()
	{
		base._Unequipped();
		On = false;
	}

	public override void _Used()
	{
		base._Used();
		On ^= true;
	}

	public override void _Ready()
	{
		if (!Engine.IsEditorHint())
		{ 
			Model = this.GetGameNode<MeshInstance3D>("%Player/Arm/%Flashlight");

			Lights = Model.GetChild<Node3D>(0);
			Lights.Visible = On;

			Energy = Lights.GetChild<Light3D>(0).LightEnergy;
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (!Engine.IsEditorHint() && Lights is not null && Lights.Visible)
		{
			foreach (Light3D light in Lights.GetChildren().Cast<Light3D>())
			{
				float guh = Energy * EnergyMult;

				light.LightEnergy = this.Twlerp(
					light.LightEnergy,
					Mathf.Clamp(light.LightEnergy - GD.RandRange(-1, 1), guh - 1, guh + 1),
					1 / 1.5f,
					delta
				);
			}
		}
	}
}
