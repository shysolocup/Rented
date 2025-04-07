using Godot;
using System;
using Godot.Collections;

[GlobalClass]
public partial class DialogueLine : Resource
{
	[Export] public string Text;
	public AudioStreamPlayer Audio;
	public Array<DialogueButton> Buttons = new();
	public Array<Button> NodeButtons = new();
}
