using Godot;

// DisplayServer.WindowGetSize();

[Tool]
[GlobalClass, Icon("uid://cb7frf5mruesy")]
public partial class UDim2Anchor : Node
{

    [ExportGroup("Operations")]

    [Export] public bool Canvas { 
        get => canvas; 
        set {
            canvas = value;
            Update(); 
        }
    }
    [Export] public bool AnchorSize {
        get => anchorSize; 
        set {
            anchorSize = value;
            Update(); 
        }
    }
    [Export] public bool AnchorPosition {
        get => anchorPos; 
        set {
            anchorPos = value;
            Update(); 
        }
    }
    [ExportToolButton("Update")] public Callable UpdateCall => Callable.From(Update);
    private bool canvas = false;
    private bool anchorSize = false;
    private bool anchorPos = false;


    [ExportGroup("Dynamics")]

    [Export] public UDim2 Size { 
        get => size; 
        set {
            size = value;
            Update(); 
        }
    }
    [Export] public UDim2 Position { 
        get => pos; 
        set {
            pos = value;
            Update();
        }
    }
    [Export] public Vector2 AnchorPoint { 
        get => anchor; 
        set {
            anchor = value;
            Update(); 
        }
    }
    private UDim2 size = UDim2.DefaultSize;
    private UDim2 pos = UDim2.Zero;
    private Vector2 anchor = new(0.5f, 0.5f);

    public void guh<T>() where T : Control
    {

    }

    public void Update()
    {
        Vector2 basis;

        if (GetParent() is Control c) {
            basis = Canvas ? DisplayServer.WindowGetSize() : c.GetParentAreaSize(); // probably works
        }
    }
}