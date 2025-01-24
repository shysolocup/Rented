using Godot;
using System;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class DialogueCharacter : Resource
{
    [Export] public string Id;
    [Export(PropertyHint.MultilineText)] public string DisplayName;
    [Export(PropertyHint.MultilineText)] public Array<string> Wraps;
    [Export] public Color Color = new(0,0,0);
    [Export] public Font Font;

    [Export] public string Path;

    [Export] public Dictionary<string, DialogueLine> Lines;
}