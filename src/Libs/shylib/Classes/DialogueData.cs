using Godot;
using System;
using Godot.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

[Tool]
[GlobalClass]
public partial class DialogueData : Node
{
	private string _path = "res://src/Data/Dialog/";

	[Export] public string Path {
		get { return _path; }
		set {
			_path = value;
			_Ready();
		}
	}

	[Export] public DialogueCharacter CharacterEditor;
	[Export] public Dictionary<string, DialogueCharacter> Characters = new();
	public VBoxContainer DialogueContainer;
	public VBoxContainer Base;
	private float id = 0;


	public async Task<VBoxContainer> PlayByInstance(DialogueCharacter character, Array<DialogueLine> LineSequence) {
		var inst = Base.Duplicate() as VBoxContainer;

		var chara = inst.GetChild<RichTextLabel>(0);
		var text = inst.GetChild<RichTextLabel>(1);

		inst.Name = id.ToString().Replace(".", "");
		id += 0.1f;

		DialogueContainer.AddChild(inst);

		foreach ( DialogueLine line in LineSequence) {
			chara.Text = string.Format(chara.Text, line.Text);
			text.Text = string.Format(text.Text, line.Text);

			while (!Input.IsActionJustPressed("dialogic_default_action")) await Task.Delay(25);
		}

		return inst;
	}

	public async Task<VBoxContainer> Play(string character, string line) {
		DialogueCharacter charData = Characters[character];
		Array<DialogueLine> lineData = charData.Lines[line];

		return await PlayByInstance(charData, lineData);
	}

	public async Task<VBoxContainer> PlayByCharacterInstance(DialogueCharacter character, string line) {
		Array<DialogueLine> lineData = character.Lines[line];
		
		return await PlayByInstance(character, lineData);
	}


	public override void _Ready() 
	{
		DialogueContainer = GetParent<VBoxContainer>().GetChild<VBoxContainer>(1);
		Base = DialogueContainer.GetNode<VBoxContainer>("Base");

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

				var lines = (Dictionary<string, Variant>)data["Lines"];

				var character = new DialogueCharacter() {
					Id = (string)data["Id"],
					DisplayName = (string)data["DisplayName"],
				};

				foreach ( (string lineId, Variant rawLineData) in lines) {
					var linesData = (Array<Array<string>>)rawLineData;

					foreach( Array<string> lineData in linesData ) {
						var line = new DialogueLine() {
							Text = lineData[0],
							Audio = (lineData[1] is string audio && audio.Length > 0) ? GetNode<AudioStreamPlayer>(audio) : null
						};

						character.Lines[lineId].Add(line);
					}
				}

				Characters[character.Id] = character;

				charFolder = chars.GetNext();
			}
		}

		else {
			GD.Print("An error occurred when trying to access the path.");
		}
	}
}
