using Godot;
using System;
using Godot.Collections;

[GlobalClass, Icon("uid://bhib8x7fhxxwd")]
public partial class DiscordController : Node
{

	static public Node DiscordRPCNode;

	static public Dictionary<string, DiscordStatus> Statuses = [];

	static private string _status = "default";

	[Export] public string Status {
		get {
			return _status;
		}
		set {

		}
	}

	public void SetStatus() 
	{

	}

	public override void _Ready()
	{
		base._Ready();

		GDScript rpcscript = GD.Load<GDScript>("res://src/Libs/shylib/Classes/Discord/DiscordRPCNode.gd");
		DiscordRPCNode = (Node)rpcscript.New();

		RefreshRPC();
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
