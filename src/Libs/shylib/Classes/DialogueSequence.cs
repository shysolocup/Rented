using Godot;
using System;
using Godot.Collections;

[GlobalClass]
public partial class DialogueSequence : Resource
{
	[Export] public string Character = "";
	[Export] public Array<DialogueLine> Lines = new();

	[Signal] public delegate void LineStartedEventHandler(DialogueLine line);
	[Signal] public delegate void EndedEventHandler();


	static public Array<DialogueSequence> EvalConvos(Variant sequences)
	{
		Array<DialogueSequence> arr = new();

		foreach ( Variant sequenceData in (Array<Variant>)sequences) {

			string character = (string)((Array<Variant>)sequenceData)[0];
			Variant linesVar = ((Array<Variant>)sequenceData)[1];

			DialogueSequence sequence = new() {
				Character = character
			};

			if (linesVar.VariantType == Variant.Type.Array) {
				Array<Variant> lines = (Array<Variant>)((Array<Variant>)sequenceData)[1];

				foreach ( Variant rawLineData in lines ) {
					sequence.Lines.Add(DialogueLine.Eval(rawLineData));
				};
			}
			else {
				sequence.Lines.Add(DialogueLine.Eval(linesVar));
			}

			arr.Add(sequence);
		}

		return arr;
	}


	static public Array<DialogueSequence> EvalInteracts(Variant sequences) 
	{
		DialogueSequence sequence = new() {
			Character = "",
		};

		if (sequences.VariantType == Variant.Type.Array) {
			foreach ( Variant lineData in (Array<Variant>)sequences) {
				sequence.Lines.Add(DialogueLine.Eval(lineData));
			};
		}
		else {
			sequence.Lines.Add(DialogueLine.Eval(sequences));
		}

		Array<DialogueSequence> arr = new() {
			sequence
		};

		return arr;
	}
}
