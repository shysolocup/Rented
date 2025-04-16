using Godot;
using System;
using Godot.Collections;

public partial class StateChapter : Resource
{
    [Export] public string Name = "epoch whatever booboo this isn't meant to be seen";
    public Dictionary<string, State> States = [];
}