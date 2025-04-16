using Godot;
using System;
using Godot.Collections;

public partial class State : Resource
{
    static public State Default = new State() {
        Status = DiscordStatus.Default
    };

    public string Id = "guh";
    [Export] public string Place = "guh";
    [Export] public DiscordStatus Status = DiscordStatus.Default;
}