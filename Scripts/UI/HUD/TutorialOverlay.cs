using Godot;

namespace Menus.Overlays;

public partial class TutorialOverlay : Control {
	public override async void _Ready() {
		Control popUp = GetNode<Control>("PopUp");
		popUp.Visible = false;
		GetTree().Paused = true;

		AnimationPlayer blackBackgroundAnimationPlayer = GetNode<AnimationPlayer>("BlackBackground/AnimationPlayer");
		blackBackgroundAnimationPlayer.Play("fade");
		await ToSignal(blackBackgroundAnimationPlayer, AnimationPlayer.SignalName.AnimationFinished);

		popUp.Visible = true;
	}

	public void OnCloseButtonPressed() {
		GetTree().Paused = false;
		QueueFree();
	}
}
