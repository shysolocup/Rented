using Godot;
using System;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class MiniCallable : Resource
{
    static public Dictionary<string, Callable> CallsCache = new Dictionary<string, Callable>() {
        ["test"] = Callable.From( () => {
            
        })
    }

    [ExportToolButton("Call")] public Callable CallButton => Callable.From(Call);

    public Variant Call()
    {
        var source = GD.Load(Source);


        if (source is CSharpScript cs) cs.Call()
    }
}