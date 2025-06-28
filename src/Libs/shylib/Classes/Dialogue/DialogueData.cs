using Godot;
using System;
using Godot.Collections;
using System.Threading.Tasks;
using System.Threading;
using Appox;

[Tool]
[GlobalClass, Icon("uid://beseoxu7me3c0")]
public partial class DialogueData : Node
{
	static private string _path = "res://src/Data/Dialog/";
	static private readonly Random rand = new();
	static private Color Transparent = new(1, 1, 1, 0);

	public string Path {
		get => _path;
		set {
			_path = value;
			_Ready();
		}
	}

	private SceneTree tree;


	static public Dictionary<string, Array<DialogueSequence>> Scenes = [];
	static public Array<DialogueCharacterEffect> Effects = [];
	public VBoxContainer DialogueContainer;
	private VBoxContainer Base;
	private TextureRect Background;
	private string BaseCharacterText;
	private string BaseText; 
	private Button BaseButton;
	private TextureRect Top;
	private TextureRect Bottom;
	private float id = 0;

	[Signal] public delegate void StartDialogueEventHandler(Array<DialogueSequence> scene);

	[Signal] public delegate void FinishDialogueEventHandler(Array<DialogueSequence> scene);



	public override void _EnterTree()
	{
		base._EnterTree();
		tree = GetTree();
	}


	#region FadeEffect
	private async Task FadeEffect(RichTextLabel label, DialogueLine line, CancellationToken token)
	{
		string realText = DialogueCharacterEffect.Apply(line.Text);
		GD.Print(realText);

		for (int i = 0; i < line.Text.Length; i++)
		{
			while (tree.Paused) await Task.Delay(10, token);
			if (token.IsCancellationRequested) break;

			label.Text = string.Format(BaseText, i, i + 1, realText);

			await Task.Delay(line.Speed, token);
		}

		token.ThrowIfCancellationRequested();
	}
	#endregion


	private float alpha = 0;
	private float factor = 1/20f;

	private Color HandleColor(Color c, double delta)
	{
		return new Color(c, this.Twlerp(c.A, alpha, factor, delta));
	}

	#region _Process
	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint()) return;
		if (alpha != Top.Modulate.A) {
			Top.Modulate = HandleColor(Top.Modulate, delta);
			Bottom.Modulate = HandleColor(Bottom.Modulate, delta);
			Background.Modulate = HandleColor(Background.Modulate, delta);
		}

		base._Process(delta);
	}
	#endregion


	#region PlayByInstance
	public async Task PlayByInstance(Array<DialogueSequence> scene, CancellationToken token = new(), VBoxContainer inst = null) {
		bool recursive = inst != null;

		if (!Engine.IsEditorHint()) {
			alpha = 1;
			
			inst ??= Base.Duplicate() as VBoxContainer;
			inst.Show();

			var buttons = inst.GetChild<HFlowContainer>(2);
			var elipses = inst.GetChild<RichTextLabel>(3);

			// clear all the buttons already in the container
			buttons.ClearChildren();

			var chara = inst.GetChild<RichTextLabel>(0);
			var textlabel = inst.GetChild<RichTextLabel>(1);

			textlabel.Text = "";

			inst.Name = id.ToString().Replace(".", "");
			id += 0.1f;

			if (!recursive) {
				DialogueContainer.AddChild(inst);
				DialogueContainer.MoveChild(inst, 1);
			}

			EmitSignalStartDialogue(scene);

			foreach ( DialogueSequence sequence in scene) {
				if (token.IsCancellationRequested) break;

				foreach (DialogueLine line in sequence.Lines) {

					await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
					await ToSignal(tree, SceneTree.SignalName.ProcessFrame);

					if (token.IsCancellationRequested) break;

					if (sequence.Character.Trim().Length > 0) {
						chara.Text = DialogueCharacterEffect.Apply(string.Format(BaseCharacterText, sequence.Character));
						chara.Show();
					}
					else chara.Hide();

					using var tokenSource = new CancellationTokenSource();
					var token2 = tokenSource.Token;

					#region Play Random
					if (line.Randoms.Count > 0) {
						int r = rand.Next(line.Randoms.Count);
						Array<DialogueSequence> redir = line.Randoms[r];
						Task thread = PlayByInstance(redir, token, inst);
						await thread.WaitAsync(token);
						GD.Print(redir, thread);
						continue;
					}
					#endregion

					Task effect = FadeEffect(textlabel, line, token2);

					buttons.Hide();
					elipses.Visible = line.Skippable;

					#region Play Redirect
					if (line.Redirect != null) {
						GD.Print('a');
						Task thread = Play(line.Redirect, token, inst);
						await thread.WaitAsync(token);
						GD.Print(thread);
					}
					#endregion

					#region Play Buttons
					else if (line.Buttons.Count > 0) {
						bool done = false;
						buttons.Show();
						elipses.Hide();

						
						foreach (DialogueButton btnData in line.Buttons) {
							if (token.IsCancellationRequested) break;

							Button btn = BaseButton.Duplicate() as Button;
							buttons.AddChild(btn);
							btn.Modulate = Transparent;
							int index = btn.GetIndex();

							btn.Text = DialogueCharacterEffect.Apply(btnData.Text);
							btn.Pressed += async () => {
								if (done) return;

								foreach (Button otherBtns in buttons.GetChildren()) {
									otherBtns.Disabled = true;

									if (otherBtns.GetIndex() != index) {
										Tween otherBtnTween = CreateTween();
										otherBtnTween.Finished += () => { otherBtns.Free(); otherBtnTween.Dispose(); };
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
								await Play(btnData.RedirectLine, token, inst);

								// hopefully will stop the loop when a button is pressed
								done = true;
							};

							Tween buttontween = CreateTween();
							buttontween.Finished += () => buttontween.Dispose();
							buttontween.TweenProperty(btn, "modulate:a", 1, 1)
							.SetTrans(Tween.TransitionType.Quad)
							.SetEase(Tween.EaseType.Out);
						}

						while (!done) { 
							await Task.Delay(10);
							if (token.IsCancellationRequested) break;
						};
					}
					#endregion

					#region Play Normal
					else {
						/*
							stays paused while
							- not pressing an interact key
							- game is paused
							- line animation isn't done
						*/
						while (!token.IsCancellationRequested && (!Input.IsActionJustPressed("InteractDialog") || tree.Paused || (!line.Skippable && !effect.IsCompleted))) await Task.Delay(10);
						
						elipses.Show();
						tokenSource.Cancel();
						tokenSource.Dispose();

						await tree.CreateTimer(0.1f).Guh();
					}
					#endregion
				}
			}

			if (!recursive) {
				alpha = 0;
				inst.Free();
			}
		}

		EmitSignalFinishDialogue(scene);

		token.ThrowIfCancellationRequested();
	}
	#endregion

	#region Misc Plays
	public async Task Play(string scene = "interact_default", CancellationToken token = new(), VBoxContainer inst = null) {
		await PlayByInstance(GetScene(scene), token, inst);
	}
	#endregion


	#region GetScene
	public Array<DialogueSequence> GetScene(string scene = "interact_default")
	{
		return Scenes.ContainsKey(scene) ? Scenes[scene] : Scenes["interact_nonexistent"];
	}
	#endregion


	#region FetchDialogs
	public Dictionary<string, Array<DialogueSequence>> FetchDialogs()
	{
		using var dialogueDir = DirAccess.Open(Path);

		foreach (string file in dialogueDir.GetFiles()) {
			using var fileData = FileAccess.Open(Path + file, FileAccess.ModeFlags.Read);

			var fileContent = new Json();
			fileContent.Parse(Jsonc.Minify(fileData.GetAsText()));
			var content = (
				Dictionary<string, Variant>
			)fileContent.Data;

			DialogueFormat format = (DialogueFormat)(int)content["format"];

			if (format == DialogueFormat.Convo || format == DialogueFormat.Interact) {
				string prefix = (string)content["prefix"];
				Dictionary<string, Variant> scenes = (Dictionary<string, Variant>)content["scenes"];

				switch(format) {
					case DialogueFormat.Convo: {
						foreach ( (string scene, Variant sequences) in scenes) {
							Scenes[$"{prefix}_{scene}"] = DialogueSequence.EvalConvos(sequences);
						}
						break;
					}

					default: {
						foreach ( (string scene, Variant sequences) in scenes) {
							Scenes[$"{prefix}_{scene}"] = DialogueSequence.EvalInteracts(sequences);
						}
						break;
					}
				}
			}

			else if (format == DialogueFormat.CharacterEffect) {
				var effectList = (Array<Dictionary<string, Variant>>)content["effects"];

				foreach (Dictionary<string, Variant> charef in effectList)
				{
					DialogueCharacterEffect effect = new();

					effect.Names = (Array<string>)charef["names"];
					effect.Mentioned = (string)charef["mentioned"];
					
					Effects.Add(effect);
				}
			}
		}

		GD.Print(Scenes);

		return Scenes;
	}
	#endregion


	#region _Ready
	public override void _Ready() 
	{
		Node parent = GetParent();

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
