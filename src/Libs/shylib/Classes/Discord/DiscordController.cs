using Godot;
using System;
using Godot.Collections;
using System.Threading.Tasks;

[GlobalClass, Icon("uid://bhib8x7fhxxwd")]
public partial class DiscordController : Node
{

	static public Node DiscordRPCNode;
	private PlaceController pc;

	public DiscordController SetStatus(Place place)
	{
		DiscordStatus status = place.Status;

		GD.Print(status.LargeImage.ToString());
		GD.Print($"\"{status.State}\"");
		GD.Print(place.Epoch.Title);

		bool hasSmallImage = status.SmallImage != DiscordStatusImage.none;

		DiscordRPCNode.Set("Details", status.Details ?? default);
		DiscordRPCNode.Set("State", status.State.Trim() == "" ? place.Epoch.Title : status.State);
		DiscordRPCNode.Set("LargeImage", status.LargeImage.ToString() ?? default);
		DiscordRPCNode.Set("LargeImageText", status.LargeImageText ?? default);
		DiscordRPCNode.Set("SmallImage", hasSmallImage ? status.LargeImage.ToString() ?? default : "");
		DiscordRPCNode.Set("SmallImageText", hasSmallImage ? status.SmallImageText ?? default : "");
		
		return this;
	}

	public override async void _Ready()
	{
		base._Ready();

		GDScript rpcscript = GD.Load<GDScript>("res://src/Libs/shylib/Classes/Discord/DiscordRPCNode.gd");
		DiscordRPCNode = (Node)rpcscript.New();

		pc = GetNode<PlaceController>("../%PlaceController");

		await ToSignal(pc, Node.SignalName.Ready);

		pc.PlaceChanged += () => {
			SetStatus(pc.Place).RefreshRPC();
		};

		SetStatus(pc.Place).RefreshRPC();
		GD.Print(IsWorking());
	}

	public bool IsWorking() 
	{
		return (bool)DiscordRPCNode.Call("IsWorking");
	}

	public void RefreshRPC()
	{
		DiscordRPCNode.Call("RefreshRPC");
	}

	public void NewTimestamp()
	{
		DiscordRPCNode.Call("NewTimestamp");
	}
}
