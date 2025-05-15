using Godot;

// DisplayServer.WindowGetSize();

[Tool]
[GlobalClass, Icon("uid://cb7frf5mruesy")]
public partial class UDim2Anchor : Node
{

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


    [ExportGroup("Operations")]

    [Export] public bool Canvas { 
        get => canvas; 
        set {
            canvas = value;
            Update(); 
        }
    }
    [ExportToolButton("Update")] public Callable UpdateCall => Callable.From(Update);
    private bool canvas = false;


    public void Update()
    {
        if (GetParent() is Control me) {
            Vector2 basis = me.GetParentAreaSize(); // probably works
        }
    }
}