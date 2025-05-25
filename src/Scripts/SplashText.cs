using Godot;
using System;

public partial class SplashText : RichTextLabel
{
	private Random random = new Random();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	/*
	shitty gpt code guhhh
	
	private string GetRandomText()
	{
		float total = 0f;
		foreach (var weight in textOptions.Values)
		{
			total += weight;
		}

		float roll = (float)(random.NextDouble() * total);
		float cumulative = 0f;

		foreach (var pair in textOptions)
		{
			cumulative += pair.Value;
			if (roll <= cumulative)
				return pair.Key;
		}

		// Fallback (shouldn't happen if weights are set correctly)
		return "Default Text";
	}*/
}
