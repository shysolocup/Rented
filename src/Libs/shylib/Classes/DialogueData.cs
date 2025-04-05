using Godot;
using System;
using Godot.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

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
	private VBoxContainer Base;
	private string BaseCharacterText;
	private string BaseText; 
	private TextureRect Top;
	private TextureRect Bottom;
	private float id = 0;


	private async Task FadeEffect(RichTextLabel label, string text, CancellationToken token)
	{
		for (int i = 0; i < text.Length; i++) {
			if (token.IsCancellationRequested) break;

			await Task.Delay(30);
			
			label.Text = string.Format(BaseText, i, i+1, text); 
		}

		token.ThrowIfCancellationRequested();
	}


	public async Task PlayByInstance(DialogueCharacter character, Array<DialogueLine> LineSequence) {
		Tween toptween = Top.CreateTween();
		toptween.Finished += () => toptween.Dispose();
		toptween.TweenProperty(Top, "modulate:a", 1, 1.3f)
		.SetTrans(Tween.TransitionType.Quad)
		.SetEase(Tween.EaseType.Out);

		Tween bottomtween = Top.CreateTween();
		bottomtween.Finished += () => bottomtween.Dispose();
		bottomtween.TweenProperty(Bottom, "modulate:a", 1, 1.3f)
		.SetTrans(Tween.TransitionType.Quad)
		.SetEase(Tween.EaseType.Out);
		
		var inst = Base.Duplicate() as VBoxContainer;
		inst.Show();

		var chara = inst.GetChild<RichTextLabel>(0);
		var textlabel = inst.GetChild<RichTextLabel>(1);

		inst.Name = id.ToString().Replace(".", "");
		id += 0.1f;

		DialogueContainer.AddChild(inst);
		DialogueContainer.MoveChild(inst, 1);

		if (character.DisplayName.Trim().Length > 0) {
			chara.Text = string.Format(BaseCharacterText, character.DisplayName);
		}
		else chara.Hide();

		foreach ( DialogueLine line in LineSequence) {
			using var tokenSource = new CancellationTokenSource();
			var token = tokenSource.Token;

			textlabel.Text = string.Format(BaseText, 0, 0, line.Text);

			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			await GetTree().CreateTimer(0.05f).Guh();

			Task effect = FadeEffect(textlabel, line.Text, token);

			while (!Input.IsActionJustPressed("InteractDialog")) { await Task.Delay(5); };

			tokenSource.Cancel();

			await GetTree().CreateTimer(0.1f).Guh();
		}

		if (IsInstanceValid(toptween) && toptween.IsRunning()) toptween.Stop();
		if (IsInstanceValid(bottomtween) && bottomtween.IsRunning()) bottomtween.Stop();

		toptween = Top.CreateTween();
		toptween.Finished += () => toptween.Dispose();
		toptween.TweenProperty(Top, "modulate:a", 0, 1)
		.SetTrans(Tween.TransitionType.Quad)
		.SetEase(Tween.EaseType.Out);

		bottomtween = Top.CreateTween();
		bottomtween.Finished += () => bottomtween.Dispose();
		bottomtween.TweenProperty(Bottom, "modulate:a", 0, 1)
		.SetTrans(Tween.TransitionType.Quad)
		.SetEase(Tween.EaseType.Out);

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
		
		Top = DialogueContainer.GetNode<TextureRect>("Top");
		Bottom = DialogueContainer.GetNode<TextureRect>("Bottom");

		Top.Modulate = new Color(1, 1, 1, 0);
		Bottom.Modulate = new Color(1, 1, 1, 0);

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
