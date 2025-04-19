#if TOOLS

using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class Zendex : EditorPlugin
{
    static readonly public string DockDir = "res://addons/zendex/ZendexDock.tscn";
    static readonly public string PluginDir = "res://addons/zendex/ZendexPlugin.tscn";

    static private Control DockUi;
    static private Control PluginUi;

    public void SetupDock() 
    {
        if (IsInstanceValid(DockUi) || DockUi != null) {
            RemoveControlFromDocks(DockUi);
            DockUi.QueueFree();
            DockUi = null;
        }

        DockUi = GD.Load<PackedScene>(DockDir).Instantiate<Control>();

        DockUi.GetNode("ReloadContainer").GetChild<Button>(0).Pressed += () => {
            SetupDock();
        };
        
        AddControlToDock(DockSlot.RightUl, DockUi);
        SetupSettings();
    }

    public void SetupPlugin() 
    {
        if (IsInstanceValid(PluginUi) || PluginUi != null) {
            RemoveControlFromBottomPanel(PluginUi);
            PluginUi.QueueFree();
            PluginUi = null;
        }

        PluginUi = GD.Load<PackedScene>(PluginDir).Instantiate<Control>();

        PluginUi.GetNode("ReloadContainer").GetChild<Button>(0).Pressed += () => {
            SetupPlugin();
        };
        
        AddControlToBottomPanel(DockUi, "Zendex");
        SetupSettings();
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        SetupPlugin();
        SetupDock();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        RemoveControlFromBottomPanel(PluginUi);
        RemoveControlFromDocks(DockUi);
        PluginUi.QueueFree();
        PluginUi = null;
        DockUi.QueueFree();
        DockUi = null;
    }

    public override void _DisablePlugin()
    {
        base._DisablePlugin();
        RemoveControlFromBottomPanel(PluginUi);
        RemoveControlFromDocks(DockUi);
        PluginUi.QueueFree();
        PluginUi = null;
        DockUi.QueueFree();
        DockUi = null;
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