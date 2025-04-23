using Godot;
using CoolGame;

public partial class SaveIconSpinner : Sprite2D
{
	

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
