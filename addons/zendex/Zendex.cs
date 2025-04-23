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
        if (IsInstanceValid(DockUi)) {
            RemoveControlFromDocks(DockUi);
            DockUi.QueueFree();
        }
        if (DockUi is not null) DockUi = null;

        DockUi = GD.Load<PackedScene>(DockDir).Instantiate<ZendexDock>();

        DockUi.GetNode("ReloadContainer").GetChild<Button>(0).Pressed += () => {
            SetupDock();
        };
        
        AddControlToDock(DockSlot.RightUl, DockUi);
    }

    public void SetupPlugin() 
    {
        if (IsInstanceValid(PluginUi)) {
            RemoveControlFromBottomPanel(PluginUi);
            PluginUi.QueueFree();
        }
        if (PluginUi is not null) PluginUi = null;

        PluginUi = GD.Load<PackedScene>(PluginDir).Instantiate<Control>();

        PluginUi.GetNode("ReloadContainer").GetChild<Button>(0).Pressed += () => {
            SetupPlugin();
        };

        PluginUi.GetNode("EntireContainer").GetChild(0).GetChild<Button>(1).Pressed += () => {
            SetupDock();
        };
        
        AddControlToBottomPanel(PluginUi, "Zendex");
    }

    public void Setup() 
    {
        if (!Engine.IsEditorHint()) return;
        SetupPlugin();
        SetupDock();
        SetupSettings();

        Selection = EditorInterface.Singleton.GetSelection();

        Selection.SelectionChanged += SelectionChanged;
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        if (Engine.IsEditorHint()) Setup();
    }

    private void SelectionChanged() {
        if (!Engine.IsEditorHint()) return;
        if (Selection is null || !IsInstanceValid(DockUi) || DockUi is null) Setup();
        DockUi.SelectionChanged(Selection.GetSelectedNodes());
    }

    public override void _ExitTree()
    {
        RemoveControlFromBottomPanel(PluginUi);
        RemoveControlFromDocks(DockUi);

        if (IsInstanceValid(DockUi)) DockUi.QueueFree();
        if (IsInstanceValid(PluginUi)) PluginUi.QueueFree();

        PluginUi = null;
        DockUi = null;

        base._ExitTree();
    }

    public override void _DisablePlugin()
    {
        base._DisablePlugin();

        RemoveControlFromBottomPanel(PluginUi);
        RemoveControlFromDocks(DockUi);

        if (IsInstanceValid(DockUi)) DockUi.QueueFree();
        if (IsInstanceValid(PluginUi)) PluginUi.QueueFree();

        PluginUi = null;
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