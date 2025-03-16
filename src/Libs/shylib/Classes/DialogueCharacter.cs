using Godot;
using System;
using Godot.Collections;

[GlobalClass]
public partial class DialogueCharacter : Resource
{
    [Export] public string Id;
    [Export(PropertyHint.MultilineText)] public string DisplayName;
    [Export] public Color Color = new(0,0,0,1);
    [Export] public Font Font;

    [Export] public string Path;

    [Export] public Dictionary<string, DialogueLine> Lines = new();
}