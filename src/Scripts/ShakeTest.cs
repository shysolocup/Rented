using Godot;

public partial class ShakeTest : Node
{
    private Shaker3D shaker;

    public override async void _Ready()
    {
        shaker = GetParent<Shaker3D>();

        while (true) {
            shaker.Start();
            await ToSignal(shaker, Shaker3D.SignalName.Finished);
        }
    }
}