using Godot;
using System;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class DialogueLine : Resource
{
    public DialogueCharacter Character;
    
    [Export(PropertyHint.MultilineText)] public string Text; 
    [Export] public Color Color = new(0,0,0);
    [Export] public Font Font;
}