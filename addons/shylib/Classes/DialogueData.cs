using Godot;
using System;
using Godot.Collections;
using System.Collections.Immutable;
using System.Linq;

[Tool]
[GlobalClass]
public partial class DialogueData : Node
{
    private string _path = "res://src/Dialog/";

    [Export] public string Path {
        get { return _path; }
        set {
            _path = value;
            _Ready();
        }
    }

    [Export] public DialogueCharacter CharacterEditor;
    [Export] public DialogueLine DialogueEditor;

    [Export] public Dictionary<string, DialogueCharacter> Characters = new();

    public override void _Ready() 
    {
        using var chars = DirAccess.Open(Path);
        
        if (chars != null) {
            chars.ListDirBegin();
            string charFolder = chars.GetNext();

            if (charFolder.EndsWith(".json")) {
                using var chardata = FileAccess.Open(Path + "/" + charFolder, FileAccess.ModeFlags.Read);
                var character = new DialogueCharacter();

                var json = new Json();
                json.Parse(chardata.GetAsText());

                var data = (Dictionary<string, Variant>)json.Data;

                var color1 = (Array<int>)data["Color"];
                var lines = (Dictionary<string, Variant>)data["Lines"];

                foreach ( (string lineId, Variant lineData) in lines) {
                    var line = new DialogueLine();

                    
                }

                character.Color = new Color(color1[0], color1[1], color1[2], color1[3]);

                Characters.Add(character.Id, character);

                charFolder = chars.GetNext();
            }
        }

        else {
            GD.Print("An error occurred when trying to access the path.");
        }
    }
}