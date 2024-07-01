#nullable enable
using Godot;
using SceneController;

namespace UI;

public partial class SceneTransition : ColorRect {
    private static readonly PackedScene _prefab = GD.Load<PackedScene>("res://Prefabs/UI/SceneTransition.tscn");

    public static SignalAwaiter FadeIn(Node? parent = null) {
        SceneTransition transition = _prefab.Instantiate<SceneTransition>();

        AnimationPlayer animationPlayer = transition.GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.Play(name: "fade_in");
        parent ??= BaseScene.UICanvas;
        parent.AddChild(transition);

        return transition.ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
    }

	public static void FadeOut(Node? parent = null) {
        SceneTransition transition = _prefab.Instantiate<SceneTransition>();

        transition.GetNode<AnimationPlayer>("AnimationPlayer").PlayBackwards("fade_in");
        parent ??= BaseScene.UICanvas;
        parent.AddChild(transition);
    }

    public void OnDurationEnded(StringName animationName) => QueueFree();
}
