#if TOOLS
using Godot;
using System;

[Tool]
public partial class GDSharpFixes : EditorPlugin {}


public static class Extensions
{
    public static Vector2 Snapped(this Vector2 vector, Vector2 gridSize)
    {
        return new Vector2(
            Mathf.Floor(vector.X / gridSize.X) * gridSize.X,
            Mathf.Floor(vector.Y / gridSize.Y) * gridSize.Y
        );
    }
}

#endif