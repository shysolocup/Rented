using Godot;
using System;
using Godot.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.VisualBasic;

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


	public Dictionary<string, Array<DialogueSequence>> Lines = new();
	public VBoxContainer DialogueContainer;
	private VBoxContainer Base;
	private TextureRect Background;
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


	public async Task PlayByInstance(Array<DialogueSequence> sequences) {
		Tween toptween = CreateTween();
		toptween.Finished += () => toptween.Dispose();
		toptween.TweenProperty(Top, "modulate:a", 1, 1.3f)
		.SetTrans(Tween.TransitionType.Quad)
		.SetEase(Tween.EaseType.Out);

		Tween bottomtween = CreateTween();
		bottomtween.Finished += () => bottomtween.Dispose();
		bottomtween.TweenProperty(Bottom, "modulate:a", 1, 1.3f)
		.SetTrans(Tween.TransitionType.Quad)
		.SetEase(Tween.EaseType.Out);

		Tween bgtween = CreateTween();
		bgtween.Finished += () => bgtween.Dispose();
		bgtween.TweenProperty(Background, "modulate:a", 1, 1.3f)
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

		foreach ( DialogueSequence sequence in sequences) {
			if (sequence.Character.Trim().Length > 0) {
				chara.Text = string.Format(BaseCharacterText, sequence.Character);
				chara.Show();
			}
			else chara.Hide();

			foreach (DialogueLine line in sequence.Lines) {
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
		}

		if (IsInstanceValid(toptween) && toptween.IsRunning()) toptween.Stop();
		if (IsInstanceValid(bottomtween) && bottomtween.IsRunning()) bottomtween.Stop();

		toptween = CreateTween();
		toptween.Finished += () => toptween.Dispose();
		toptween.TweenProperty(Top, "modulate:a", 0, 1)
		.SetTrans(Tween.TransitionType.Quad)
		.SetEase(Tween.EaseType.Out);

		bottomtween = CreateTween();
		bottomtween.Finished += () => bottomtween.Dispose();
		bottomtween.TweenProperty(Bottom, "modulate:a", 0, 1)
		.SetTrans(Tween.TransitionType.Quad)
		.SetEase(Tween.EaseType.Out);

		bgtween = CreateTween();
		bgtween.Finished += () => bgtween.Dispose();
		bgtween.TweenProperty(Background, "modulate:a", 0, 1)
		.SetTrans(Tween.TransitionType.Quad)
		.SetEase(Tween.EaseType.Out);

		inst.Free();
	}

	public async Task Play(string line = "interact_default") {
		await PlayByInstance(Lines[line]);
	}

	public async Task PlayByCharacterInstance(string line) {		
		await PlayByInstance(Lines[line]);
	}


	public override void _Ready() 
	{
		Node parent = GetParent();

		DialogueContainer = parent.GetChild<VBoxContainer>(1);
		Background = parent.GetChild<TextureRect>(2);
		
		Top = DialogueContainer.GetNode<TextureRect>("Top");
		Bottom = DialogueContainer.GetNode<TextureRect>("Bottom");

		Top.Modulate = new Color(1, 1, 1, 0);
		Bottom.Modulate = new Color(1, 1, 1, 0);
		Background.Modulate = new Color(1, 1, 1, 0);

		Base = DialogueContainer.GetNode<VBoxContainer>("Base");
		BaseCharacterText = Base.GetChild<RichTextLabel>(0).Text;
		BaseText = Base.GetChild<RichTextLabel>(1).Text;

		if (!Engine.IsEditorHint()) {
			DialogueContainer.GetNode<Control>("Base").Visible = false;
		}

		using var convodata = FileAccess.Open("res://src/Data/Dialog/Convos.json", FileAccess.ModeFlags.Read);

		var convojson = new Json();
		convojson.Parse(convodata.GetAsText());
		var convos = (
			Dictionary<string, 
				Array< // line:[], line:[]
					Array<Variant> // "character, []
				>
			>
		)convojson.Data;

		foreach ( (string lineid, Array<Array<Variant>> sequences) in convos) {
			Lines[$"convo_{lineid}"] = new();

			foreach ( Array<Variant> sequenceData in sequences) {
				string character = (string)sequenceData[0];
				Array<Variant> lines = (Array<Variant>)sequenceData[1];

				DialogueSequence sequence = new() {
					Character = character
				};

				foreach ( Variant rawLineData in lines ) {

					DialogueLine line;
					
					if (rawLineData.VariantType == Variant.Type.Array) {
						Array<string> lineArr = (Array<string>)rawLineData;

						line = new DialogueLine() {
							Text = lineArr[0],
							Audio = (lineArr.ElementAtOrDefault(1) is string audio && audio.Length > 0) ? GetNode<AudioStreamPlayer>(audio) : null
						};
					}
					else {
						line = new DialogueLine() {
							Text = (string)rawLineData,
							Audio = null
						};
					}

					sequence.Lines.Add(line);
				};

				Lines[$"convo_{lineid}"].Add(sequence);
			}
		}

		using var interactdata = FileAccess.Open("res://src/Data/Dialog/Interacts.json", FileAccess.ModeFlags.Read);

		var interactjson = new Json();
		interactjson.Parse(interactdata.GetAsText());
		var interacts = (
			Dictionary<string, Array<string>>
		)interactjson.Data;

		foreach ( (string lineid, Array<string> sequences) in interacts) {
			Lines[$"interact_{lineid}"] = new();

			DialogueSequence sequence = new() {
				Character = "",
			};

			foreach ( string text in sequences) {
				sequence.Lines.Add(new DialogueLine() {
					Text = text
				});
			};

			Lines[$"interact_{lineid}"].Add(sequence);
		}

		GD.Print(Lines);
	}
}
