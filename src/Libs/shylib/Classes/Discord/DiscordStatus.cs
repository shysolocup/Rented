using Godot;
using System;

[GlobalClass, Icon("uid://bhib8x7fhxxwd")]
public partial class DiscordStatus : Resource
{

    static public DiscordStatus Default = new DiscordStatus() {
        Details = "lorem ipsum",
        State = "you're not meant to see this",
        LargeImage = DiscordStatusImage.placeholder
    };

    [Export] public string Details = "lorem ipsum";
    [Export] public string State = "not meant to see this";
    [Export] public DiscordStatusImage LargeImage = DiscordStatusImage.placeholder;
    [Export] public string LargeImageText = "placeholder";
    [Export] public DiscordStatusImage SmallImage = DiscordStatusImage.placeholder;
    [Export] public string SmallImageText = "placeholder";
    [Export] public int StartTimestamp;
    [Export] public int EndTimestamp;
}