using Godot;

[GlobalClass, Icon("uid://bwcxp8wfwv51d")]
public partial class UDim2 : Resource
{

    [Export] public Vector2 X = Vector2.Zero;
    [Export] public Vector2 Y = Vector2.Zero;


    public static UDim2 operator + (UDim2 a, dynamic b) => new(a.X + b.X, a.Y + b.Y);
    public static UDim2 operator * (UDim2 a, dynamic b) => new(a.X * b.X, a.Y * b.Y);
    public static UDim2 operator / (UDim2 a, dynamic b) => new(a.X / b.X, a.Y / b.Y);
    public static UDim2 operator ^ (UDim2 a, dynamic b) => new(a.X ^ b.X, a.Y ^ b.Y);
    public static UDim2 operator % (UDim2 a, dynamic b) => new(a.X % b.X, a.Y % b.Y);


    static public UDim2 Zero { get => new(0, 0, 0, 0); }
    static public UDim2 DefaultSize { get => new(0, 200, 0, 200); }


    public UDim2 Lerp(UDim2 to, float weight) => new(X.Lerp(to.X, weight), Y.Lerp(to.Y, weight));


    public UDim2() : base() {}

    public UDim2(float xScale, float xOffset, float yScale, float yOffset) : base()
    {
        X = new(xScale, xOffset);
        Y = new(yScale, yOffset);
    }

    public UDim2(Vector2 X, Vector2 Y) : base() 
    {
        this.X = X;
        this.Y = Y;
    }

    static public UDim2 FromScale(float xScale, float yScale) => new(xScale, 0, yScale, 0);
    static public UDim2 FromOffset(float xOffset, float yOffset) => new(0, xOffset, 0, yOffset);
}