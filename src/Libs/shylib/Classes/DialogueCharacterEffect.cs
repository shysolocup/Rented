using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class DialogueCharacterEffect : Resource
{
    /*
        Character effects highlights a character's name and text when they speak or are mentioned
        eg: 
            Name = "Hass" Mentioned = "[b]{0}[/b]"
            now if a dialogue were to mention "Hass" it would be in bold
            it would also bold if a dialogue where to do [hass]text[/hass]
    */

    [Export] public Array<string> Names = new();
    [Export] public string Mentioned = "{0}";
    [Export] public string Speaking = "{0}";
}