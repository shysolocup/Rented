using Godot;
using System;
using Godot.Collections;

[GlobalClass, Icon("uid://n4vqp518c1tw")]
public partial class Place : Resource
{
    static public Place Default = new Place() {
        Status = DiscordStatus.Default
    };

    [Export] public string Display = "guh";
    [Export] public DiscordStatus Status = DiscordStatus.Default;
    [Export] public PlaceEpoch Epoch;
}