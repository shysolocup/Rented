using Godot;
using System;
using CoolGame;

public partial class SaveIconSpinner : Sprite2D
{
	// Called when the node enters the scene tree for the first time.

	// public Tween rotation = new Tween();

	public void Saving()
	{
		// rotation.TweenProperty(this, "rotation", 360, 1);
	}

	private const float followSpeed = 4.0f;

	public override async void _Ready()
	{
		await Game.Init();

		Visible = false;

		Game.Instance.Saved += () => {
			// rotation.Stop();
		};

		Saving();

		/*await Game.Init();
		GD.Print(Game.Saves);

		Game.Instance.Saving += Saving;
		
		*/
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
