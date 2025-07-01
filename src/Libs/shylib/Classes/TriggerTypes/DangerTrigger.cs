using CoolGame;
using Godot;

public partial class DangerTrigger : Trigger3D
{
    private Player Player;

    public override void _Ready()
    {
        base._Ready();
        Player = this.GetGameNode<Player>("%Player");
    }

    public override void _Touched(Node diddler)
    {
        base._Touched(diddler);
        Player.InDanger = true;
    }

    public override void _TouchEnded(Node diddler)
    {
        base._TouchEnded(diddler);
        Player.InDanger = false;
    }
}