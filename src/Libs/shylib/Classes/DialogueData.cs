using Godot;
using System;
using Godot.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

[Tool]
[GlobalClass]
public partial class DialogueData : Node
{
	private string _path = "res://src/Data/Dialog/";
	private Random rand = new Random();
	private Color Transparent = new Color(1, 1, 1, 0);

	[Export] public string Path {
		get { return _path; }
		set {
			_path = value;
			_Ready();
		}
	}

	private SceneTree tree;


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
	[Signal] public delegate void FinishDialogEventHandler();


	public override void _EnterTree()
	{
		base._EnterTree();
		tree = GetTree();
	}


	#region FadeEffect
	private async Task FadeEffect(RichTextLabel label, DialogueLine line, CancellationToken token, int speed = 30)
	{
		for (int i = 0; i < line.Text.Length; i++) {
			while (tree.Paused) await Task.Delay(5);
			if (token.IsCancellationRequested) break;

			await Task.Delay(line.Speed);
			
			label.Text = string.Format(BaseText, i, i+1, line.Text); 
		}

		token.ThrowIfCancellationRequested();
	}
	#endregion


	#region LineEval
	private DialogueLine LineEval(Variant lineData)
	{
		switch (lineData.VariantType) {

			// Array 
			// ["text", "audio", [["button1", "redirline1], ["button2", "redirline1"]]]
			#region LineEval:Array
			case Variant.Type.Array: {
				Array<Variant> lineArr = (Array<Variant>)lineData;

				DialogueLine line = new() {
					Text = (string)lineArr[0],
					Audio = (lineArr.ElementAtOrDefault(1).Obj is string audio && audio.Length > 0) ? GetNode<AudioStreamPlayer>(audio) : null
				};

				if (lineArr.ElementAtOrDefault(2).VariantType == Variant.Type.Array) {
					foreach (Array<string> btn in (Array<Array<string>>)lineArr[2]) {
						line.Buttons.Add(new DialogueButton() {
							Text = btn.ElementAtOrDefault(0),
							RedirectLine = btn.ElementAtOrDefault(1)
						});
					}
				}
				else if (lineArr.ElementAtOrDefault(2).VariantType == Variant.Type.String) {
					line.Buttons.Add(new DialogueButton() {
						Text = (string)lineArr[2],
					});
				}

				return line;
			}
			#endregion

			//
			#region LineEval:Dict
			case Variant.Type.Dictionary: {
				Dictionary<string, Variant> lineDict = (Dictionary<string, Variant>)lineData;

				DialogueLine line = new() {
					Text = (string)lineDict["text"],
					Audio = (lineDict.ContainsKey("audio") && lineDict["audio"].GetType() == typeof(AudioStreamPlayer))? (AudioStreamPlayer)lineDict["audio"] : null
				};

				#region LE:Dict:Btns
				if (lineDict.ContainsKey("buttons")) {
					foreach (Variant btnVar in (Array<Variant>)lineDict["buttons"]) {
						GD.Print(btnVar);

						if (btnVar.VariantType == Variant.Type.Array) {
							Array<string> btn = (Array<string>)btnVar;

							line.Buttons.Add(new DialogueButton() {
								Text = btn.ElementAtOrDefault(0),
								RedirectLine = btn.ElementAtOrDefault(1)
							});
						}

						else if (btnVar.VariantType == Variant.Type.String) {
							line.Buttons.Add(new DialogueButton() {
								Text = (string)btnVar,
							});
						}
					}
				}
				#endregion

				if (lineDict.ContainsKey("redirect")) {
					line.Redirect = (string)lineDict["redirect"];
				}

				line.Skippable = lineDict.ContainsKey("skippable") ? (bool)lineDict["skippable"] : true;
				line.Speed = lineDict.ContainsKey("speed") ? (int)lineDict["speed"] : DialogueLine.DefaultSpeed;
				line.EvalType = lineDict.ContainsKey("eval") ? (DialogueEvalType)(int)lineDict["speed"] : DialogueEvalType.Interact;

				#region LE:Dict:Rand
				if (lineDict.ContainsKey("random")) {
					foreach (Variant randomLine in (Array<Variant>)lineDict["random"]) {
						switch (line.EvalType) {
							case DialogueEvalType.Convo: {
								line.Randoms.Add(EvalConvos((Array<Array<Variant>>)randomLine));
								break;
							}
							case DialogueEvalType.Interact: {
								line.Randoms.Add(EvalInteracts((Array<Variant>)randomLine));
								break;
							}
						}
					}
				}
				#endregion

				return line;
			}
			#endregion

			#region LineEval:Default
			default: return new DialogueLine() {
				Text = (string)lineData
			};
			#endregion

		};
	}
	#endregion


	#region PlayByInstance
	public async Task<bool> PlayByInstance(Array<DialogueSequence> sequences, VBoxContainer inst = null) {
		bool recursive = inst != null;

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
			var elipses = inst.GetChild<RichTextLabel>(3);

			// clear all the buttons already in the container
			foreach (Button button in buttons.GetChildren()) {
				buttons.RemoveChild(button);
				button.QueueFree();
			}

			var chara = inst.GetChild<RichTextLabel>(0);
			var textlabel = inst.GetChild<RichTextLabel>(1);

			inst.Name = id.ToString().Replace(".", "");
			id += 0.1f;

			if (!recursive) {
				DialogueContainer.AddChild(inst);
				DialogueContainer.MoveChild(inst, 1);
			}

			foreach ( DialogueSequence sequence in sequences) {
				foreach (DialogueLine line in sequence.Lines) {
					if (sequence.Character.Trim().Length > 0) {
						chara.Text = string.Format(BaseCharacterText, sequence.Character);
						chara.Show();
					}
					else chara.Hide();

					using var tokenSource = new CancellationTokenSource();
					var token = tokenSource.Token;

					#region PBI:Random
					if (line.Randoms.Count > 0) {
						int r = rand.Next(line.Randoms.Count);
						await PlayByInstance(line.Randoms.ElementAtOrDefault(r));
						continue;
					}
					#endregion

					textlabel.Text = string.Format(BaseText, 0, 0, line.Text);

					await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
					await tree.CreateTimer(0.05f).Guh();

					Task effect = FadeEffect(textlabel, line, token);

					buttons.Hide();
					elipses.Visible = line.Skippable;

					#region PBI:Redirect
					if (line.Redirect != null) {
						await Play(line.Redirect, inst);
						await tree.CreateTimer(0.1f).Guh();
					}
					#endregion

					#region PBI:Buttons
					else if (line.Buttons.Count > 0) {
						bool done = false;
						buttons.Show();
						elipses.Hide();

						
						foreach (DialogueButton btnData in line.Buttons) {
							Button btn = BaseButton.Duplicate() as Button;
							buttons.AddChild(btn);
							btn.Modulate = Transparent;
							int index = btn.GetIndex();

							btn.Text = btnData.Text;
							btn.Pressed += async () => {
								if (done) return;

								foreach (Button otherBtns in buttons.GetChildren()) {
									otherBtns.Disabled = true;

									if (otherBtns.GetIndex() != index) {
										Tween otherBtnTween = CreateTween();
										otherBtnTween.Finished += () => otherBtnTween.Dispose();
										otherBtnTween.TweenProperty(otherBtns, "modulate:a", 0, 0.2f)
										.SetTrans(Tween.TransitionType.Quad)
										.SetEase(Tween.EaseType.Out);

										otherBtnTween.Play();
									}
								}

								await tree.CreateTimer(0.3f).Guh();

								Tween buttontween = CreateTween();
								buttontween.Finished += () => buttontween.Dispose();
								buttontween.TweenProperty(btn, "modulate:a", 0, 1f)
								.SetTrans(Tween.TransitionType.Quad)
								.SetEase(Tween.EaseType.Out);

								buttontween.Play();

								await ToSignal(buttontween, Tween.SignalName.Finished);

								await tree.CreateTimer(0.1f).Guh();
								await Play(btnData.RedirectLine, inst);

								// hopefully will stop the loop when a button is pressed
								done = true;
							};

							Tween buttontween = CreateTween();
							buttontween.Finished += () => buttontween.Dispose();
							buttontween.TweenProperty(btn, "modulate:a", 1, 1)
							.SetTrans(Tween.TransitionType.Quad)
							.SetEase(Tween.EaseType.Out);
						}

						while (!done) { await Task.Delay(5); };
					}
					#endregion

					#region PBI:Default
					else {
						/*
							stays paused while
							- not pressing an interact key
							- game is paused
							- line animation isn't done
						*/
						while (!Input.IsActionJustPressed("InteractDialog") || tree.Paused || (!line.Skippable && !effect.IsCompleted)) await Task.Delay(5);
						
						elipses.Show();
						tokenSource.Cancel();

						await tree.CreateTimer(0.1f).Guh();
					}
					#endregion
				}
			}

			if (toptween != null && IsInstanceValid(toptween) && toptween.IsRunning()) toptween.Stop();
			if (bottomtween != null && IsInstanceValid(bottomtween) && bottomtween.IsRunning()) bottomtween.Stop();
			if (bgtween != null && IsInstanceValid(bgtween) && bgtween.IsRunning()) bgtween.Stop();

			if (!recursive) {
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

		return true;
	}
	#endregion

	#region Misc Plays
	public async Task<bool> Play(string line = "interact_default", VBoxContainer inst = null) {
		return await PlayByInstance(Lines[line], inst);
	}

	public async Task<bool> PlayByCharacterInstance(string line, VBoxContainer inst = null) {		
		return await PlayByInstance(Lines[line], inst);
	}
	#endregion


	#region FetchDialogs
	public Dictionary<string, Array<DialogueSequence>> FetchDialogs()
	{

		#region Convos
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
			Lines[$"convo_{lineid}"] = EvalConvos(sequences);
		}
		#endregion


		#region Interacts
		using var interactdata = FileAccess.Open("res://src/Data/Dialog/Interacts.json", FileAccess.ModeFlags.Read);

		var interactjson = new Json();
		interactjson.Parse(interactdata.GetAsText());
		var interacts = (
			Dictionary<string, Array<Variant>>
		)interactjson.Data;

		foreach ( (string lineid, Array<Variant> sequences) in interacts) {
			Lines[$"interact_{lineid}"] = EvalInteracts(sequences);
		}
		#endregion


		#region Deaths
		using var deathsdata = FileAccess.Open("res://src/Data/Dialog/Deaths.json", FileAccess.ModeFlags.Read);

		var deathsjson = new Json();
		deathsjson.Parse(deathsdata.GetAsText());
		var deaths = (
			Dictionary<string, Array<Variant>>
		)deathsjson.Data;

		foreach ( (string lineid, Array<Variant> sequences) in deaths) {
			Lines[$"death_{lineid}"] = EvalInteracts(sequences);
		}
		#endregion

		GD.Print(Lines);

		return Lines;
	}
	#endregion


	#region EvalConvos
	public Array<DialogueSequence> EvalConvos(Array<Array<Variant>> sequences)
	{
		Array<DialogueSequence> arr = new();

		foreach ( Array<Variant> sequenceData in sequences) {
			string character = (string)sequenceData[0];
			Array<Variant> lines = (Array<Variant>)sequenceData[1];

			DialogueSequence sequence = new() {
				Character = character
			};

			foreach ( Variant rawLineData in lines ) {
				sequence.Lines.Add(LineEval(rawLineData));
			};

			arr.Add(sequence);
		}

		return arr;
	}
	#endregion


	#region EvalInteracts
	public Array<DialogueSequence> EvalInteracts(Array<Variant> sequences) 
	{
		DialogueSequence sequence = new() {
			Character = "",
		};

		foreach ( Variant lineData in sequences) {
			sequence.Lines.Add(LineEval(lineData));
		};

		Array<DialogueSequence> arr = new() {
			sequence
		};

		return arr;
	}
	#endregion


	#region _Ready
	public override void _Ready() 
	{
		Node parent = GetParent();

		GD.Print("a");

		DialogueContainer = parent.GetChild<VBoxContainer>(1);
		Background = parent.GetChild<TextureRect>(2);
		
		Top = DialogueContainer.GetNode<TextureRect>("Top");
		Bottom = DialogueContainer.GetNode<TextureRect>("Bottom");

		if (!Engine.IsEditorHint()) {
			Top.Modulate = Transparent;
			Bottom.Modulate = Transparent;
			Background.Modulate = Transparent;
		}

		Base = DialogueContainer.GetNode<VBoxContainer>("Base");
		BaseCharacterText = Base.GetChild<RichTextLabel>(0).Text;
		BaseText = Base.GetChild<RichTextLabel>(1).Text;
		HFlowContainer BaseButtonContainer = Base.GetChild<HFlowContainer>(2);
		BaseButton = BaseButtonContainer.GetChild<Button>(0);

		if (!Engine.IsEditorHint()) {
			DialogueContainer.GetNode<Control>("Base").Visible = false;
			BaseButtonContainer.Hide();
		}

		FetchDialogs();
	}
	#endregion
}
