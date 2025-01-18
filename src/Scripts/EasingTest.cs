using Godot;
using System;
using System.Linq;

public partial class EasingTest : Button
{

	public Tween.TransitionType[] types = {
		Tween.TransitionType.Linear,
		Tween.TransitionType.Sine,
		Tween.TransitionType.Cubic
	};

	public int i = 0;
	public RichTextLabel thing;

	public bool easing = false;
	public float goal;
	public Tween.TransitionType type;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		thing = GetNode<RichTextLabel>("../RichTextLabel");
	}

	public override void _Pressed()
	{
		goal = this.Position.X == 0 ? GetViewportRect().Size.X : 0;
		type = types[new RandomNumberGenerator().RandiRange(0, types.Count()-1)];
		this.Text = type.ToString();
		easing = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (easing) {
			thing.Position = this.Lerp(thing.Position.X, goal, 1/1.5);

			if (goal == thing.Position.X) {
				easing = false;
			}
		}
	}
}
