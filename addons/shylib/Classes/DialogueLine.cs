using Godot;
using System;
using Godot.Collections;

[GlobalClass]
public partial class DialogueLine : Resource
{
    public DialogueCharacter Character;
    
    [Export(PropertyHint.MultilineText)] public string Text; 
    [Export] public Color Color = new(0,0,0,1);
    [Export] public Font Font;
    public AudioStreamPlayer Audio;
}