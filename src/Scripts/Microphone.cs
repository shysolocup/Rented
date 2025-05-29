using Godot;


[GlobalClass]
public partial class Microphone : AudioStreamPlayer
{
	[Signal] public delegate void StartedSpeakingEventHandler();
	[Signal] public delegate void StoppedSpeakingEventHandler();

	static public float Volume = 0;

	static public bool Speaking
	{
		get => Volume > 0;
	}

	static private int AudioIndex = AudioServer.GetBusIndex("Microphone");

	public override void _Process(double delta)
	{
		Volume = Mathf.Clamp(Mathf.DbToLinear(AudioServer.GetBusPeakVolumeLeftDb(AudioIndex, 0)) * 130, 0, 100);
	}
}
