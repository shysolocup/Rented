using System.Threading.Tasks;
using Godot;

[GlobalClass, Icon("uid://e1srvp36hc7n")]
public partial class InteractObject3D : RigidBody3D
{
	[Export] public bool Enabled = true;
	[Export] public bool Locked = false;
	[Export] public StandardMaterial3D HoverIcon;
	[Export] public bool AutoCooldown = true;
	[Export] public float CooldownDuration = 1;
	public bool Cooldown = false;
	private Player Player;

	private bool _pressed = false;
	private bool _hovering = false;


	[Signal] public delegate void PressChangedEventHandler();
	[Signal] public delegate void HoverChangedEventHandler();

	public async void CooldownHandle()
	{
		if (AutoCooldown) {
			Cooldown = true;
			await GetTree().CreateTimer(CooldownDuration).Guh();
			Cooldown = false;
		}
	}


	public virtual void _PressChanged() 
	{
		CooldownHandle();
	}
	public virtual void _HoverChanged() {}
	
	private Crosshair3D Crosshair; 

	public override void _Ready()
	{
		Crosshair = this.GetGameNode<Crosshair3D>("%CrosshairGui/%Crosshair");
		Player = this.GetGameNode<Player>("%Player");
	}


	public override void _Input(InputEvent @event) 
	{
		if (Input.IsActionJustPressed("Interact") && Enabled && Hovering && !Player.InDialog && !Cooldown) Pressed = true;
		else Pressed = false;
	}



	[Export] public bool Pressed {
		get => _pressed;

		set {
			if (value != _pressed) {
				_pressed = !Locked && Enabled && !Cooldown && value;
				_PressChanged();
				EmitSignalPressChanged();
			}
		}
	}

	[Export] public bool Hovering {
		get => _hovering;

		set {
			if (value != _hovering) {
				_hovering = !Cooldown && value;

				if (_hovering && Enabled) {
					Crosshair.Icon.SetSurfaceOverrideMaterial(0, HoverIcon ?? Crosshair.DefaultHoverIcon);
				}
				else if (_hovering && Enabled && Locked) {
					Crosshair.Icon.SetSurfaceOverrideMaterial(0, Crosshair.LockedIcon);
				}
				else if (!_hovering) {
					Crosshair.Icon.SetSurfaceOverrideMaterial(0, Crosshair.DefaultIcon);
				}

				_HoverChanged();
				EmitSignalHoverChanged();
			}
		}
	}
}
