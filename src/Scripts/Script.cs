using Godot;
using System;

public partial class Script : Node
{
	public Node GetNodeChild( Node node, string name ) 
	{
		return node.GetNode(new NodePath(name));
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var guh = GetParent() as CharacterBody3D;
		var mesh = GetNodeChild(guh, "Mesh") as MeshInstance3D;

		GD.Print(guh.Name);
		GD.Print(mesh.Name);

		Tween tween = CreateTween();

		tween.Finished += () => GD.Print("tween done!!");

		tween.TweenProperty(mesh, "rotation", new Vector3(1.5f, 1.5f, 1.5f), 5).SetTrans(Tween.TransitionType.Bounce);
		tween.Parallel();
		tween.TweenProperty(mesh, "scale:z", 5, 5).SetTrans(Tween.TransitionType.Spring);

		tween.Play();
	}

	/* I'll use you later you little fucker

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}*/
}
