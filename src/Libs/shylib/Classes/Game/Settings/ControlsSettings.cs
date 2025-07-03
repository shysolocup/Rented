using Godot;

public partial class ControlsSettings : GameSetting
{
    /*private float SensitivityValue;
    private PanelContainer SensitivityContainer;*/


    public override void _Ready()
    {
        base._Ready();

        var container = GetChild(0);

        // SensitivityContainer = container.GetNode<PanelContainer>("Sensitivity");
    }
}