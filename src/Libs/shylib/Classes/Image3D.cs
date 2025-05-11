using Godot;

[Tool]
[GlobalClass, Icon("uid://ck61ilinbabbn")]
public partial class Image3D : MeshInstance3D
{
    private StandardMaterial3D mat = new();
    private Texture2D source;
    private float imageScale = 1;


    [Export] public Texture2D Source {
        get => source;
        set {
            if (source == value) return;
            source = value;
            Changed();
        }
    }

    [Export] public bool Reference = false;

    [Export(PropertyHint.Range, "0,2,or_greater,or_less")] public float ImageScale {
        get => imageScale;
        set {
            if (imageScale == value) return;
            imageScale = value;
            Scale = Rescale();
        }
    }

    public Vector3 Rescale() {
        Vector2 scale = source.GetSize() / 500 * imageScale;
        return new Vector3(scale.X, 1, scale.Y);
    }

    public void Changed()
    {
        if (source is not null) {
            Scale = Rescale();
        }

        mat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;

        CastShadow = ShadowCastingSetting.Off;

        mat.AlbedoTexture = source;
    }

    public override void _Ready()
    {
        base._Ready();

        if (Reference && !Engine.IsEditorHint()) Hide();

        Mesh ??= new PlaneMesh();
        Mesh.SurfaceSetMaterial(0, mat);

        Changed();
    }
}