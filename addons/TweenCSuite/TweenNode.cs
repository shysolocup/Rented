

using Godot;
using System;
using Godot.Collections;

/// <summary>
/// Node that uses [<see cref="Tween"/>] for animating.
/// TweenNode is a [<see cref="Node"/>] wrapper for [<see cref="Tween"/>]. you can put it in your scene and configure from the inspector. The node can either use [<see cref="TweenAnimation"/>] resource or you can setup it from code (see [<see cref="TweenNode.InitializeAnimation()"/>]). The [<see cref="Tween"/>] is created automatically at the end of the frame when [<see cref="TweenNode"/>] is ready and is automatically bound to this node. it will also be valid as long as the node exists.
/// </summary>
 
[Tool]
[GlobalClass, Icon("uid://4yavgmppmd22")]
public partial class TweenNode : Node
{
    const string _EARLY_ERROR = "Method called to early, Tween not initialized. Use MakeTween() to force early creation.5";

    private TweenAnimation _animation;

    /// <summary>
    /// The resource that defines the [<see cref="Tween"/>]'s animation.
    /// </summary>
    [Export]
    public TweenAnimation Animation {
        
        get { return _animation; }
        set {
            if (value == _animation) return;

            _animation = value;
            NotifyPropertyListChanged();

            if (_tween is not null) {
                _tween.Kill();
                _tween = null;
                MakeTween();
            }
        }
    }

    /// <summary>   
    /// the path to the root node of the animation, relative to this node. All [<see cref="TweenAnimation"/>] paths use the root node as base. This is similar to <see cref="AnimationMixer.RootNode"/>.
    /// Only usable when [<see cref="Node"/>] is assigned. it has no effect otherwise.
    /// </summary>
    [Export] 
    public string AnimationRoot = "..";

    [ExportGroup("Initial Settings")]

    /// <summary>
    /// If <code>AutoStart = true</code>, the [<see cref="Tween"/>] will start automatically.
    /// </summary>
    [Export] 
    public bool AutoStart = false;

    /// <summary>
    /// Speed of the [<see cref="Tween"/>] animations. Equivalent of using [<see cref="Tween.SetSpeedScale()"/>].
    /// </summary>
    [Export] 
    public float SpeedScale = 1;


    /// <summary>
    /// Default transition for the [<see cref="Tween"/>]'s tweeners. See [<see cref="Tween.TransitionType"/>].
    /// </summary>
    [Export]
    public Tween.TransitionType DefaultTransition = Tween.TransitionType.Linear;



    /// <summary>
    /// Default easing for the [<see cref="Tween"/>]'s tweeners. see [<see cref="Tween.EaseType"/>].
    /// </summary>
    [Export]
    public Tween.EaseType DefaultEasing = Tween.EaseType.InOut;

    /// <summary>
    /// If <code>Parallel = true</code> the [<see cref="Tween"/>]'s animation will be parallel by default (see [<see cref="Tween.SetParallel()"/>]). Has no effect when [<see cref="TweenNode.Animation"/>] is assigned.
    /// </summary>
    [Export]
    public bool Parallel = false;

    [ExportGroup("Processing Settings")]

    /// <summary>
    /// Number of animation loops the [<see cref="Tween"/>] will do before stopping. <code>0</code> means infinite.
    /// </summary>
    [Export(PropertyHint.Range, "0,9999")] 
    public int Loops = 1;

    /// <summary>
    /// The frame during which the [<see cref="Tween"/>] will be processing. See [<see cref="Tween.TweenProcessMode"/>]
    /// </summary>
    [Export]
    public Tween.TweenProcessMode TweenProcessMode = Tween.TweenProcessMode.Idle;
}