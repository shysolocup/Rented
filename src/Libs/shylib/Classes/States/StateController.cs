using Godot;
using System;
using Godot.Collections;
using System.Linq;

public partial class StateController : Node 
{

	[Signal] public delegate void StateChangedEventHandler(State Old);
	[Signal] public delegate void ChapterChangedEventHandler(StateChapter Old);

	static public Array<StateChapter> Chapters = [];

	static private State _state;
	static private StateChapter _chapter;

    [Export] public State State {
		get { return _state; }
		set {
			if (value != _state) {
				State oldstate = _state;
				_state = value;
				EmitSignal(SignalName.StateChanged, oldstate);
			}
		}
	}
    [Export] public StateChapter Chapter {
		get { return _chapter; }
		set {
			if (value != _chapter) {
				StateChapter oldchapt = _chapter;
				_chapter = value;
				EmitSignal(SignalName.ChapterChanged, oldchapt);
			}
		}
	}

    static private string _path = "res://src/Data/States.json";
	static private Color Transparent = new Color(1, 1, 1, 0);

	public string Path {
		get { return _path; }
		set {
			_path = value;
			_Ready();
		}
	}

    public Array<StateChapter> FetchStates()
	{
		using var fileData = FileAccess.Open(Path, FileAccess.ModeFlags.Read);

		var fileContent = new Json();
		fileContent.Parse(fileData.GetAsText());
		var content = (
			Dictionary<string, Variant>
		)fileContent.Data;

		Array<Dictionary<string, Variant>> chapters = (Array<Dictionary<string, Variant>>)content["chapters"];

		foreach (Dictionary<string, Variant> chapterData in chapters) {
			StateChapter chapter = new() {
				Name = (string)chapterData["name"]
			};

			foreach (var stateData in (Array<Dictionary<string, Variant>>)chapterData["states"]) {
				State state = new() {
					Id = (string)stateData["id"],
					Place = (string)stateData["place"],

					Status = new() {
						Details = (string)stateData["details"],
						State = stateData.GetValueOrDefault<string>("")
					}
				};
			}
		}

		GD.Print(Chapters);

		return Chapters;
	}

    public override void _Ready()
    {
        base._Ready();

        Node parent = GetParent();

		/*DialogueContainer = parent.GetChild<VBoxContainer>(1);
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
		}*/

		FetchStates();
    }
}