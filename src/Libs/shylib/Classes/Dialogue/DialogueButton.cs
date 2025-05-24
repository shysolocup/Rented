using Godot;

[GlobalClass, Icon("uid://beseoxu7me3c0")]
public partial class DialogueButton : Resource
{
	[Export] public string Text = "Press";
	[Export] public string RedirectLine; 
	public bool Effectable = true;
	public Button Button;
}
