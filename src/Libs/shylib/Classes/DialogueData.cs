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
	private Button BaseButton;
	private TextureRect Top;
	private TextureRect Bottom;
	private float id = 0;


	private async Task FadeEffect(RichTextLabel label, DialogueLine line, CancellationToken token)
	{
		for (int i = 0; i < line.Text.Length; i++) {
			if (token.IsCancellationRequested) break;

			await Task.Delay(30);
			
			label.Text = string.Format(BaseText, i, i+1, line.Text); 
		}

		token.ThrowIfCancellationRequested();
	}



	private DialogueLine LineEval(Variant lineData)
	{
		switch (lineData.VariantType) {
			case Variant.Type.Array: {
				Array<string> lineArr = (Array<string>)lineData;

				return new DialogueLine() {
					Text = lineArr[0],
					Audio = (lineArr.ElementAtOrDefault(1) is string audio && audio.Length > 0) ? GetNode<AudioStreamPlayer>(audio) : null
				};
			}

			case Variant.Type.Dictionary: {
				Dictionary<string, Variant> lineDict = (Dictionary<string, Variant>)lineData;

				DialogueLine line = new() {
					Text = (string)lineDict["text"],
					Audio = (lineDict.ContainsKey("audio") && lineDict["audio"].GetType() == typeof(AudioStreamPlayer))? (AudioStreamPlayer)lineDict["audio"] : null
				};

				if (lineDict.ContainsKey("buttons")) {
					foreach (Array<string> btn in (Array<Array<string>>)lineDict["buttons"]) {
						line.Buttons.Add(new DialogueButton() {
							Text = btn[0],
                            RedirectLine = btn[1]
						});
					}
				}

				return line;
			}

			default: return new DialogueLine() {
				Text = (string)lineData,
				Audio = null
			};
		};
	}


	public async Task PlayByInstance(Array<DialogueSequence> sequences, VBoxContainer inst = null) {
		Tween toptween = null; Tween bottomtween = null; Tween bgtween = null;

		if (!Engine.IsEditorHint()) {
			if (MathF.Round(Top.Modulate.A) < 1 ) {
				toptween = CreateTween();
				toptween.Finished += () => toptween.Dispose();
				toptween.TweenProperty(Top, "modulate:a", 1, 1.3f)
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.Out);

				bottomtween = CreateTween();
				bottomtween.Finished += () => bottomtween.Dispose();
				bottomtween.TweenProperty(Bottom, "modulate:a", 1, 1.3f)
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.Out);

				bgtween = CreateTween();
				bgtween.Finished += () => bgtween.Dispose();
				bgtween.TweenProperty(Background, "modulate:a", 1, 1.3f)
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.Out);
			}
			
			inst = (inst != null) ? inst : Base.Duplicate() as VBoxContainer;
			inst.Show();

			var buttons = inst.GetChild<HFlowContainer>(2);
			buttons.GetChild(0).Free();

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

					Task effect = FadeEffect(textlabel, line, token);

					while (!Input.IsActionJustPressed("InteractDialog")) { await Task.Delay(5); };

					tokenSource.Cancel();

					if (line.Buttons.Count > 0) {
						foreach (DialogueButton btnData in line.Buttons) {
							Button btn = BaseButton.Duplicate() as Button;
							buttons.AddChild(btn);
							btn.Modulate = new Color(1, 1, 1, 0);

							btn.Text = btnData.Text;
							btn.Pressed += async () => await Play(btnData.RedirectLine);

							Tween buttontween = CreateTween();
							buttontween.Finished += () => buttontween.Dispose();
							buttontween.TweenProperty(btn, "modulate:a", 0, 1)
							.SetTrans(Tween.TransitionType.Quad)
							.SetEase(Tween.EaseType.Out);
						}
					}

					await GetTree().CreateTimer(0.1f).Guh();
				}
			}

			if (toptween != null && IsInstanceValid(toptween) && toptween.IsRunning()) toptween.Stop();
			if (bottomtween != null && IsInstanceValid(bottomtween) && bottomtween.IsRunning()) bottomtween.Stop();
			if (bgtween != null && IsInstanceValid(bgtween) && bgtween.IsRunning()) bgtween.Stop();

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
		HFlowContainer BaseButtonContainer = Base.GetChild<HFlowContainer>(2);
		BaseButton = BaseButtonContainer.GetChild<Button>(0);
		BaseButtonContainer.Hide();

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
					sequence.Lines.Add(LineEval(rawLineData));
				};

				Lines[$"convo_{lineid}"].Add(sequence);
			}
		}

		using var interactdata = FileAccess.Open("res://src/Data/Dialog/Interacts.json", FileAccess.ModeFlags.Read);

		var interactjson = new Json();
		interactjson.Parse(interactdata.GetAsText());
		var interacts = (
			Dictionary<string, Array<Variant>>
		)interactjson.Data;

		foreach ( (string lineid, Array<Variant> sequences) in interacts) {
			Lines[$"interact_{lineid}"] = new();

			DialogueSequence sequence = new() {
				Character = "",
			};

			foreach ( Variant lineData in sequences) {
				sequence.Lines.Add(LineEval(lineData));
			};

			Lines[$"interact_{lineid}"].Add(sequence);
		}

		GD.Print(Lines);
	}
}
