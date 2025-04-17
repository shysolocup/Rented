using Godot;
using Godot.Collections;
using System;
using System.Text.RegularExpressions;

[Tool]
[GlobalClass]
public partial class Trigger3D : Area3D
{
    [Signal] public delegate void TouchedEventHandler(Node node);
    [Signal] public delegate void TouchEndedEventHandler(Node node);
    [Export] public Array<Node> TouchingNodes = [];
    [Export] public bool Triggered = false;
    [Export] public bool Enabled = true;
    [Export] public Array<string> Filter = [];
    [Export] public Array<string> TypeFilter = [];
    [Export] public Array<string> DescendantFilter = [];
    [Export(PropertyHint.Enum, "Exclude:1,Include:2,")] public int FilterType = 1;

    public override void _Ready()
    {
        Monitoring = true;
        
        base._Ready();

        BodyEntered += EventHandler;
        BodyExited += EventHandler;
    }

    public void EventHandler(Node diddler)
    {
        bool flagged = false;

        foreach (string filter in Filter) {
            switch(FilterType) {
                case 1: { // Exclude
                    flagged = flagged || Regex.IsMatch(diddler.Name, filter);
                    break;
                }
                case 2: { // Include
                    flagged = flagged || !Regex.IsMatch(diddler.Name, filter);
                    break;
                }
            }
        }

        foreach (string typeFilter in TypeFilter) {
            switch(FilterType) {
                case 1: { // Exclude
                    flagged = flagged || Regex.IsMatch(diddler.GetType().Name, typeFilter);
                    break;
                }
                case 2: { // Include
                    flagged = flagged || !Regex.IsMatch(diddler.GetType().Name, typeFilter);
                    break;
                }
            }
        }
    }
}