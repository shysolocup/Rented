using Godot;
using System;

public partial class Script2 : Node
{

	public Node GetNodeChild( Node node, string name ) 
	{
		return node.GetNode(new NodePath(name));
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (!(bool)GetMeta("Enabled")) return;


		var guh = GetParent() as CharacterBody3D;
		var mesh = GetNodeChild(guh, "Mesh") as MeshInstance3D;
		
		GD.Print(guh.Name);
		GD.Print(mesh.Name);

		Tween tween = CreateTween();

		tween.SetTrans(Tween.TransitionType.Spring);
		tween.Finished += () => tweenFinished(tween);

		tween.TweenProperty(mesh, "scale", new Vector3(0.5f, 0.5f, 0.5f), 5);

		tween.Play();
	}

	public void tweenFinished(Tween tween)
	{
		GD.Print("tween done!!");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
