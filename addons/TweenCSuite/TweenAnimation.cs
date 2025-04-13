using Godot;
using System;
using Godot.Collections;

/// <summary>
/// The resource that defines [<see cref="Tween"/>] animations.
/// 
/// [<see cref="TweenAnimation"/>] can be used to animate [<see cref="Tween"/>] node without using code. Yu can edit the animation in the <code>Tweens</code> bottom tab when the resource is selected. Once created, the animation can be applied to a tween to create all [<see cref="Tweener"/>]s as defined in the animation. You can either use it with a [<see cref="TweenNode"/>] or a regular tween by calling [<see cref="TweenAnimation.ApplyToTween()"/>].   
/// </summary>

[Tool]
[GlobalClass, Icon("uid://b6nr7ridvg6xq")]
public partial class TweenAnimation : Resource
{
    public Array<Array<TweenerAnimator>> Steps = new();
    public Dictionary<StringName, Variant> Parameters = new();

    /// <summary>
    /// Applies this animation to the given [<see cref="Tween"/>]. the [<paramref name="Root"/>] is the base node for the animation paths. Called automatically when using [<see cref="TweenNode"/>].
    /// <example>
    /// This shows how to increment an integer.
    /// <code>
    ///    Tween tween = CreateTween();
    ///    TweenAnimation animation = GD.Load&lt;TweenAnimation&gt;("res://tween_animation.tres");
    ///    animation.ApplyToTween(tween, this);
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="Tween">Tween to apply to</param>
    /// <param name="Root">Root node</param>
    /// <returns>self</returns>
    public TweenAnimation ApplyToTween(Tween Tween, Node Root)
    {
        foreach (Array<TweenerAnimator> step in Steps) {
            bool first = true;

            foreach (TweenerAnimator tweener in step) {
                if (first) first = false;
                else Tween.Parallel();
            }

            Tweener.ApplyToTween(Tween, Root, this);
        }

        return this;
    }

    /// <summary>
    /// Sets a custom parameter, used for code defined values. Can be accessed with "%name" in the editor
    /// <example>
    /// This shows how to increment an integer.
    /// <code>
    ///    Tween tween = CreateTween();
    ///    TweenAnimation animation = GD.Load&lt;TweenAnimation&gt;("res://tween_animation.tres");
    ///    animation.SetParameter("MyName", 0.5f);
    ///    animation.ApplyToTween(tween, this);
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="name">name of the parameter</param>
    /// <param name="value">value of the parameter</param>
    /// <returns>self</returns>
    public TweenAnimation SetParameter(StringName name, Variant value)
    {
        Parameters[name] = value;
        return this;
    }
}