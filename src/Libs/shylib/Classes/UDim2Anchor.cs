using Godot;

// DisplayServer.WindowGetSize();

[Tool]
[GlobalClass, Icon("uid://cb7frf5mruesy")]
public partial class UDim2Anchor : Node
{
    [Export] public UDim2 Size = UDim2.Zero;
    [Export] public Vector2 AnchorPoint = Vector2.Zero;
}