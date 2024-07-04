using Godot;
using Menus.Overlays;
using SceneController;

namespace Menus.Settings;

/// <summary>
/// Because the settings button can be accessed from the pause overlay or the main menu,
/// the back button needs to be able to handle both cases.
/// </summary>
public partial class SettingsBackButton : SceneChangerButton {
    protected override void ChangeScene() {
		Node currentScene = GetTree().Root.GetChild(0);

		if (currentScene is BaseScene) { // acessed from the pause overlay
			GetParent().QueueFree(); // remove the settings menu
			GetParent().GetParent().GetNode<PauseOverlay>("PauseOverlay").GrabFocus();
		} else {
			base.ChangeScene();
		}
    }
}
