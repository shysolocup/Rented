using Godot;
using System;

[GlobalClass, Icon("uid://bhib8x7fhxxwd")]
public partial class DiscordController : Node
{

	public Node DiscordRPCNode;

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
