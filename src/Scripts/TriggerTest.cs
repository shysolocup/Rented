using Godot;

public partial class TriggerTest : Node
{
    private Trigger3D Trigger;

	public override void _Ready()
	{
		base._Ready();

        Trigger = GetParent<Trigger3D>();


        Trigger.Touched += toucher => {
            GD.Print(toucher);
        };


        Trigger.TouchEnded += toucher => {
            GD.Print(toucher);
        };
	}
}
