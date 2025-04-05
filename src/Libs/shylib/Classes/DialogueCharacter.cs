using Godot;
using System;
using Godot.Collections;

[GlobalClass]
public partial class DialogueCharacter : Resource
{
    [Export] public string Id;
    [Export(PropertyHint.MultilineText)] public string DisplayName;
    [Export] public string Path;
    [Export] public Dictionary<string, Array<DialogueLine>> Lines = new();
}