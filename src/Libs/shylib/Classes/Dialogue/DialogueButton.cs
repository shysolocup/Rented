using Godot;
using System;
using Godot.Collections;

[GlobalClass, Icon("uid://beseoxu7me3c0")]
public partial class DialogueButton : Resource
{
	[Export] public string Text = "Press";
	[Export] public string RedirectLine; 
	public Button Button;
}
