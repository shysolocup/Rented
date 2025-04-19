#if TOOLS

using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class Zendex : EditorPlugin
{
    static readonly public string SceneDir = "res://addons/zendex/ZendexUi.tscn";
    static private Control Ui;

    public void SetupDock() 
    {
        if (IsInstanceValid(Ui) || Ui != null) {
            RemoveControlFromDocks(Ui);
            Ui.QueueFree();
            Ui = null;
        }

        Ui = GD.Load<PackedScene>(SceneDir).Instantiate<ScrollContainer>();

        Ui.GetNode("ReloadContainer").GetChild<Button>(0).Pressed += () => {
            SetupDock();
        };
        
        AddControlToDock(DockSlot.RightUl, Ui);
        SetupSettings();
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        SetupDock();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        RemoveControlFromDocks(Ui);
        Ui.QueueFree();
        Ui = null;
    }

    public override void _DisablePlugin()
    {
        base._DisablePlugin();
        RemoveControlFromDocks(Ui);
        Ui.QueueFree();
        Ui = null;
    }

    private static readonly string SettingsPath = "docks/Zendex/";
    private static readonly Dictionary<string, Variant> Settings = new() {

    };

    static private void SetupSettings()
    {
        EditorSettings settings = EditorInterface.Singleton.GetEditorSettings();
        foreach ( (string setting, Variant value) in Settings ) {
            if (!settings.HasSetting(setting)) {
                settings.SetSetting(setting, value);
            }
        }
    }

}


#endif