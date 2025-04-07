using Godot;
using System;
using Godot.Collections;

[GlobalClass]
public partial class DialogueButton : Resource
{
	[Export] public string Text = "Press";
	[Export] public string RedirectLine; 
	public Button Button;
}
