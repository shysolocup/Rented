using Godot;
using System;
using Godot.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
#nullable enable

[Tool]
[GlobalClass, Icon("uid://n4vqp518c1tw")]
public partial class PlaceController : Node 
{

	[Signal] public delegate void PlaceChangedEventHandler();
	[Signal] public delegate void EpochChangedEventHandler();

	static public Dictionary<string, Place> Places = [];

	static private Place _place = Place.Default;

	[Export] public Place Place {
		get => _place;
		set {
			if (value != _place) {
				bool epochChanged = _place.Epoch != value.Epoch;
				_place = value;
				EmitSignal(SignalName.PlaceChanged);
				if (epochChanged) EmitSignal(SignalName.EpochChanged);
			}
		}
	}

	static private string _path = "res://src/Data/Places/";
	static private Color Transparent = new Color(1, 1, 1, 0);

	public string Path {
		get => _path;
		set {
			_path = value;
			_Ready();
		}
	}

	public Dictionary<string, Place> FetchPlaces()
	{

		using var dirData = DirAccess.Open(Path);

		foreach ( string file in dirData.GetFiles()) {
			using var fileData = FileAccess.Open(Path + file, FileAccess.ModeFlags.Read);

			var fileContent = new Json();
			fileContent.Parse(fileData.GetAsText());
			var content = (
				Dictionary<string, Variant>
			)fileContent.Data;


			PlaceEpoch epoch = new() {
				Title = (string)content["title"],
				Prefix = (string)content["prefix"]
			};

			foreach ( (string id, var placeData) in (Dictionary<string, Dictionary<string, Variant>>)content["places"]) {
				Place state = new() {
					Display = (string)placeData["display"],
					Epoch = epoch,
				};

				placeData.TryGetValue("state", out Variant stateVar);
				placeData.TryGetValue("large_image", out Variant largeImageVar);
				placeData.TryGetValue("large_image_text", out Variant largeImageTextVar);
				placeData.TryGetValue("small_image", out Variant smallImageVar);
				placeData.TryGetValue("small_image_text", out Variant smallImageTextVar);

				_ = Enum.TryParse((string)largeImageVar, out DiscordStatusImage largeImage);
				bool hasSmallImage = Enum.TryParse((string)smallImageVar, out DiscordStatusImage smallImage);

				DiscordStatus status = new() {
					Details = (string)placeData["details"],
					State = (string?)stateVar ?? null,
					LargeImage = largeImage,
					LargeImageText = (string?)largeImageTextVar ?? null,
					SmallImage = hasSmallImage ? smallImage : DiscordStatusImage.none,
					SmallImageText = hasSmallImage ? (string)smallImageTextVar : null
				};

				state.Status = status;

				Places[$"{epoch.Prefix}_{id}"] = state;
			}
		}

		GD.Print(Places);

		return Places;
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

		FetchPlaces();

		Place = Places["menu_default"];
	}
}
