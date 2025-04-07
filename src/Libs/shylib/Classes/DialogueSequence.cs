using Godot;
using System;
using Godot.Collections;

[GlobalClass]
public partial class DialogueSequence : Resource
{
	[Export] public string Character = "";
	[Export] public Array<DialogueLine> Lines = new();

	[Signal] public delegate void LineStartedEventHandler(DialogueLine line);
}
