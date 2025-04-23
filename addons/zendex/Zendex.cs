#if TOOLS

using Godot;
using Godot.Collections;
using System;

[Tool]
[GlobalClass]
public partial class Zendex : EditorPlugin
{
    static readonly public string DockDir = "res://addons/zendex/ZendexDock.tscn";
    static readonly public string PluginDir = "res://addons/zendex/ZendexPlugin.tscn";

    public ZendexDock DockUi;
    public Control PluginUi;
    private EditorSelection Selection;

    public void SetupDock() 
    {
        if (IsInstanceValid(DockUi) || DockUi != null) {
            RemoveControlFromDocks(DockUi);
        }

        DockUi = GD.Load<PackedScene>(DockDir).Instantiate<ZendexDock>();

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
        }

        PluginUi = GD.Load<PackedScene>(PluginDir).Instantiate<Control>();

        PluginUi.GetNode("ReloadContainer").GetChild<Button>(0).Pressed += () => {
            SetupPlugin();
        };

        PluginUi.GetNode("EntireContainer").GetChild(0).GetChild<Button>(1).Pressed += () => {
            SetupDock();
        };
        
        AddControlToBottomPanel(PluginUi, "Zendex");
        SetupSettings();
    }

    public void Setup() 
    {
        SetupPlugin();
        SetupDock();

        Selection = EditorInterface.Singleton.GetSelection();

        Selection.SelectionChanged += SelectionChanged;
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        Setup();
    }

    private void SelectionChanged() {
        if (Selection is null || DockUi is null) Setup();
        DockUi.SelectionChanged(Selection.GetSelectedNodes());
    }

    public override void _ExitTree()
    {
        RemoveControlFromBottomPanel(PluginUi);
        RemoveControlFromDocks(DockUi);
        PluginUi = null;
        DockUi = null;
        base._ExitTree();
    }

    public override void _DisablePlugin()
    {
        base._DisablePlugin();
        RemoveControlFromBottomPanel(PluginUi);
        RemoveControlFromDocks(DockUi);
        PluginUi = null;
        DockUi = null;
        base._ExitTree();
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