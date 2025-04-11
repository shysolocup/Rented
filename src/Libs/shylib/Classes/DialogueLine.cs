using Godot;
using System;
using Godot.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;

[GlobalClass]
public partial class DialogueLine : Resource
{
	static public int DefaultSpeed = 30;

	[Export] public string Text; 
	[Export] public Array<Array<DialogueSequence>> Randoms = new();
	public string Redirect;
	public bool Skippable = true;
	public int Speed = DefaultSpeed;
	public AudioStreamPlayer Audio;
	public Array<DialogueButton> Buttons = new();
	public Array<Button> NodeButtons = new();
	public DialogueFormat Format = DialogueFormat.Interact;


	static public DialogueLine Eval(Variant lineData)
	{
		switch (lineData.VariantType) {

			// Array 
			// ["text", "audio", [["button1", "redirline1], ["button2", "redirline1"]]]
			#region LineEval Array
			case Variant.Type.Array: {
				Array<Variant> lineArr = (Array<Variant>)lineData;

				DialogueLine line = new() {
					Text = (string)lineArr[0],
					// Audio = (lineArr.ElementAtOrDefault(1).Obj is string audio && audio.Length > 0) ? GetNode<AudioStreamPlayer>(audio) : null
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
			#region LineEval Dict
			case Variant.Type.Dictionary: {
				Dictionary<string, Variant> lineDict = (Dictionary<string, Variant>)lineData;

				DialogueLine line = new() {
					Text = lineDict.ContainsKey("text") ? (string)lineDict["text"] : null,
					// Audio = (lineDict.ContainsKey("audio") && lineDict["audio"].GetType() == typeof(AudioStreamPlayer)) ? (AudioStreamPlayer)lineDict["audio"] : null
				};

				#region LE Dict (Btns)
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
				line.Format = lineDict.ContainsKey("format") ? (DialogueFormat)(int)lineDict["format"] : DialogueFormat.Interact;

				#region LE Dict (Rand)
				if (lineDict.ContainsKey("random")) {
					foreach (Variant randomLine in (Array<Variant>)lineDict["random"]) {
						switch (line.Format) {
							case DialogueFormat.Convo: {
								line.Randoms.Add(DialogueSequence.EvalConvos((Array<Array<Variant>>)randomLine));
								break;
							}
							case DialogueFormat.Interact: {
								line.Randoms.Add(DialogueSequence.EvalInteracts((Array<Variant>)randomLine));
								break;
							}
						}
					}
				}
				#endregion

				return line;
			}
			#endregion

			#region LineEval Default
			default: return new DialogueLine() {
				Text = (string)lineData
			};
			#endregion

		};
	}
}
