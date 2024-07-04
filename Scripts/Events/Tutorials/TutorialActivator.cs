using Godot;
using Menus.Settings;
using SceneController;

namespace Events.Tutorials;

public partial class TutorialActivator : Node {
	[Export]
	private PackedScene _tutorialOverlay;

	[Export]
	private float _delayDuration;

	private bool _activated = false;

	public override void _Ready() {
		if (BaseScene.ProgressionController.TutorialsCompleted.Contains(this.Name)) {
			QueueFree();
		}
	}

	public void OnBodyEntered(Node body) => DisplayOverlay();

	public async void DisplayOverlay() {
		if (_activated) return;
		_activated = true;
		BaseScene.ProgressionController.TutorialsCompleted.Add(this.Name);

		if (Settings.SkipTutorials) {
			QueueFree();
			return;
		}

		if (_delayDuration > 0) {
			Control pauseBlocker = new(); // This is used to disable pausing while the timer is running.
			BaseScene.UICanvas.AddChild(pauseBlocker);
			await ToSignal(
				source: GetTree().CreateTimer(timeSec: _delayDuration),
				signal: SceneTreeTimer.SignalName.Timeout
			);
			pauseBlocker.QueueFree();
		}

		BaseScene.UICanvas.AddChild(_tutorialOverlay.Instantiate());
		QueueFree();
	}
}
