using Godot;
using SaveSystem;
using SceneController;

namespace Menus.Settings;

public partial class Settings : Control {
	public static bool FullScreenEnabled { get; private set; } = false;
	public static bool PlayerIndentifiersEnabled { get; private set; } = true;
	public static bool FriendlyFireEnabled { get; private set; } = false;
	public static bool ScreenShakeEnabled { get; private set; } = true;
	public static bool HitStopEnabled { get; private set; } = true;

	public override void _Ready() {
		GetNode<CheckButton>("SettingsBorder/FullscreenCheckButton").ButtonPressed = FullScreenEnabled;
		GetNode<CheckButton>("SettingsBorder/PlayerIdentifiersCheckBox").ButtonPressed = PlayerIndentifiersEnabled;
		GetNode<CheckButton>("SettingsBorder/FriendlyFireCheckButton").ButtonPressed = FriendlyFireEnabled;
		GetNode<CheckButton>("SettingsBorder/ScreenShakeCheckButton").ButtonPressed = ScreenShakeEnabled;
		GetNode<CheckButton>("SettingsBorder/HitstopCheckButton").ButtonPressed = HitStopEnabled;
	}

	public static void SetValuesFrom(SettingsFileModel settings) {
		FullScreenEnabled = settings.FullScreenEnabled;
		OnFullScreenButtonToggled(FullScreenEnabled);

		PlayerIndentifiersEnabled = settings.PlayerIndentifiersEnabled;
		FriendlyFireEnabled = settings.FriendlyFireEnabled;
		ScreenShakeEnabled = settings.ScreenShakeEnabled;
		HitStopEnabled = settings.HitStopEnabled;
	}

	public static void SaveSettings() {
		SaveController.SaveSettings(new SettingsFileModel(
			fullScreenEnabled: FullScreenEnabled,
			playerIndentifiersEnabled: PlayerIndentifiersEnabled,
			friendlyFireEnabled: FriendlyFireEnabled,
			screenShakeEnabled: ScreenShakeEnabled,
			hitStopEnabled: HitStopEnabled
		));
	}

	public static void OnFullScreenButtonToggled(bool toggle) {
		FullScreenEnabled = toggle;
		DisplayServer.WindowSetMode(
			mode: FullScreenEnabled ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Maximized
		);
	}

	public void OnPlayerIdentifiersButtonToggled(bool toggle) {
		PlayerIndentifiersEnabled = toggle;
		if (GetTree().CurrentScene is BaseScene) {
			BaseScene.ChangePlayerIndicatorVisibility(PlayerIndentifiersEnabled);
		}
	}

	public static void OnFriendlyFireButtonToggled(bool toggle) {
		FriendlyFireEnabled = toggle;
	}

	public static void OnScreenShakeButtonToggled(bool toggle) {
		ScreenShakeEnabled = toggle;
	}

	public static void OnHitStopButtonToggled(bool toggle) {
		HitStopEnabled = toggle;
	}
}
