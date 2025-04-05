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
	public string BaseCharacterText;
	public string BaseText; 
	private float id = 0;


	public async Task PlayByInstance(DialogueCharacter character, Array<DialogueLine> LineSequence) {
		var inst = Base.Duplicate() as VBoxContainer;
		inst.Show();

		var chara = inst.GetChild<RichTextLabel>(0);
		var text = inst.GetChild<RichTextLabel>(1);

		inst.Name = id.ToString().Replace(".", "");
		id += 0.1f;

		DialogueContainer.AddChild(inst);
		DialogueContainer.MoveChild(inst, 1);

		GD.Print(LineSequence);

		if (character.DisplayName.Trim().Length > 0) {
			chara.Text = string.Format(BaseCharacterText, character.DisplayName);
		}
		else chara.Hide();

		foreach ( DialogueLine line in LineSequence) {
			text.Text = string.Format(BaseText, line.Text);

			await Task.Delay(100);

			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

			while (!Input.IsActionJustPressed("InteractDialog")) {
				await Task.Delay(5);
			}

			GD.Print("pressed");
		}

		inst.Free();
	}

	public async Task Play(string character = "interact", string line = "default") {
		GD.Print(Characters);
		DialogueCharacter charData = Characters[character];
		Array<DialogueLine> lineData = charData.Lines[line];

		await PlayByInstance(charData, lineData);
	}

	public async Task PlayByCharacterInstance(DialogueCharacter character, string line) {
		Array<DialogueLine> lineData = character.Lines[line];
		
		await PlayByInstance(character, lineData);
	}


	public override void _Ready() 
	{
		DialogueContainer = GetParent<VBoxContainer>().GetChild<VBoxContainer>(1);
		Base = DialogueContainer.GetNode<VBoxContainer>("Base");
		BaseCharacterText = Base.GetChild<RichTextLabel>(0).Text;
		BaseText = Base.GetChild<RichTextLabel>(1).Text;

		if (!Engine.IsEditorHint()) {
			DialogueContainer.GetNode<Control>("Base").Visible = false;
		}

		using var chars = DirAccess.Open(Path);
		
		if (chars != null) {
			foreach (string charFile in chars.GetFiles()) {
				if (charFile.EndsWith(".json")) {
					using var chardata = FileAccess.Open(Path + "/" + charFile, FileAccess.ModeFlags.Read);

					var json = new Json();
					json.Parse(chardata.GetAsText());
					var data = (Dictionary<string, Variant>)json.Data;

					var lines = (Dictionary<string, Variant>)data["Lines"];

					var character = new DialogueCharacter() {
						Id = (string)data["Id"],
						DisplayName = (string)data["DisplayName"],
						Lines = new()
					};

					foreach ( (string lineId, Variant rawLineData) in lines) {
						var linesData = (Array<Array<string>>)rawLineData;
						character.Lines[lineId] = new();

						foreach( Array<string> lineData in linesData ) {
							
							var line = new DialogueLine() {
								Text = lineData[0],
								Audio = (lineData.ElementAtOrDefault(1) is string audio && audio.Length > 0) ? GetNode<AudioStreamPlayer>(audio) : null
							};

							character.Lines[lineId].Add(line);
						}
					}

					Characters[character.Id] = character;
				}
			}
		}

		else {
			GD.Print("An error occurred when trying to access the path.");
		}
	}
}
