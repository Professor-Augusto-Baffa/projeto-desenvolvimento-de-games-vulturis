using Godot;

namespace Menus.MainMenu;

public partial class MultiplayerCheckButton : CheckButton {
	public static bool IsMultiplayerActive { get; private set; } = false;

	public override void _Ready() {
		this.ButtonPressed = IsMultiplayerActive;
	}

	public static void OnToggled(bool toggledOn) {
		IsMultiplayerActive = toggledOn;
	}
}
