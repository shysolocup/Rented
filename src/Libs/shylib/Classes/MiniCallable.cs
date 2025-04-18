using Godot;
using System;
using Godot.Collections;
using Appox;

[Tool]
[GlobalClass, Icon("uid://s25d7ed1h6jd")]
public partial class MiniCallable(Variant parent) : Resource()
{
    [Export] public string CallId { get; set; }
    public Variant Parent { get; set; } = parent;

    [ExportToolButton("Call")] public Callable CallButton => Callable.From(_call);

    private void _call() => Call();

    public Variant Call(Variant? args = null)
    {
        return MiniCallableCache.Cache[CallId].Call(Parent, (Variant)args);
    }
}