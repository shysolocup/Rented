using Godot;
using System;
using Godot.Collections;

[GlobalClass]
public partial class DialogueLine : Resource
{
	static public int DefaultSpeed = 30;

	[Export] public string Text; 
	[Export] public Array<Array<DialogueSequence>> Randoms;
	public string Redirect;
	public bool Skippable = true;
	public int Speed = DefaultSpeed;
	public AudioStreamPlayer Audio;
	public Array<DialogueButton> Buttons = new();
	public Array<Button> NodeButtons = new();
	public DialogueEvalType EvalType = DialogueEvalType.Interact;
}
