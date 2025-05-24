using Godot;
using System;
using Godot.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using Appox;

[GlobalClass, Icon("uid://beseoxu7me3c0")]
public partial class DialogueCharacterEffect : Resource
{
    /*
        Character effects highlights a character's name and text when they speak or are mentioned
        eg: 
            Name = "Hass" Mentioned = "[b]{0}[/b]"
            now if a dialogue were to mention "Hass" it would be in bold
            it would also bold if a dialogue where to do [hass]text[/hass]
    */

    [Export] public Array<string> Names = [];
    [Export] public string Mentioned = "{0}";
    [Export] public string Speaking = "{0}";


	static public string Apply(string original)
	{
		Array<string> split = [.. original.Split(" ")];

		foreach (DialogueCharacterEffect effect in DialogueData.Effects) {
			foreach (string name in effect.Names)
			{
				MatchCollection matches = Regex.Matches(original, string.Format(@"\[{0}\](.*?)\[/{0}\]", name));

				foreach (Match match in matches)
				{
					GD.Print(match.Groups[1].Value);
				}

				foreach (var (word, i) in ShyUtil.Enumerate(split))
				{
					if (word.Contains(name, StringComparison.OrdinalIgnoreCase))
					{
						split[i] = string.Format(effect.Mentioned, word);
					}
				}
			}
		}

		return split.ToArray().Join(" ");
	}
}