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
	public BoxContainer DialogueContainer;
	private float id = 0;


	public RichTextLabel PlayByInstance(DialogueCharacter character, DialogueLine line) {
		var inst = DialogueContainer.GetNode("Base").Duplicate() as RichTextLabel;

		inst.Name = id.ToString().Replace(".", "");
		id += 0.1f;

		DialogueContainer.AddChild(inst);

		GD.Print(line.Color.ToHex());

		string text = string.Format(
			(string)inst.Get("bbcode"), 
			character.Color.ToHex(), // character color 
			character.DisplayName, // character name
			line.Color.ToHex(), // line color
			line.Text // line text
		);

		inst.Set("bbcode", text);

		if (line.Font != null) {
			inst.Set("font", line.Font);
		}

		inst.Visible = true;

		if (line.Audio != null) {
			line.Audio.Play();
		}

		return inst;
	}

	public RichTextLabel Play(string character, string line) {
		DialogueCharacter charData = Characters[character];
		DialogueLine lineData = charData.Lines[line];

		return PlayByInstance(charData, lineData);
	}

	public RichTextLabel PlayByCharacterInstance(DialogueCharacter character, string line) {
		DialogueLine lineData = character.Lines[line];
		
		return PlayByInstance(character, lineData);
	}


	public override void _Ready() 
	{
		DialogueContainer = GetNode<BoxContainer>("%Dialogue");

		if (!Engine.IsEditorHint()) {
			DialogueContainer.GetNode<Control>("Base").Visible = false;
		}

		using var chars = DirAccess.Open(Path);
		
		if (chars != null) {
			chars.ListDirBegin();
			string charFolder = chars.GetNext();

			if (charFolder.EndsWith(".json")) {
				using var chardata = FileAccess.Open(Path + "/" + charFolder, FileAccess.ModeFlags.Read);

				var json = new Json();
				json.Parse(chardata.GetAsText());

				var data = (Dictionary<string, Variant>)json.Data;

				var color1 = (Array<int>)data["Color"];
				var lines = (Dictionary<string, Variant>)data["Lines"];

				var character = new DialogueCharacter() {
					Id = (string)data["Id"],
					DisplayName = (string)data["DisplayName"],
					Color = new Color(color1[0], color1[1], color1[2], color1[3])
				};

				foreach ( (string lineId, Variant rawLineData) in lines) {
					var lineData = (Dictionary<string, Variant>)rawLineData;
					var color2 = (Array<int>)lineData["Color"];

					var line = new DialogueLine() {
						Text = (string)lineData["Text"],
						Color = new Color(color1[0], color1[1], color1[2], color1[3]),

						Character = character
					};

					var audio = ((string)lineData["Audio"]).Trim();
					if (audio.Length > 0) line.Audio = GetNode<AudioStreamPlayer>(audio);

					character.Lines[lineId] = line;
				}
				
				var font = ((string)data["Font"]).Trim();
				if (font.Length > 0) character.Font = GD.Load<Font>(font);

				Characters[character.Id] = character;

				charFolder = chars.GetNext();
			}
		}

		else {
			GD.Print("An error occurred when trying to access the path.");
		}
	}
}
