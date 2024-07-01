using Godot;
using SaveSystem;
using SceneController;

namespace Menus.Settings;

public partial class Settings : Control {
	private static int _musicAudioBusIndex;
	private static int _soundEffectsAudioBusIndex;

	public static bool FullScreenEnabled { get; private set; }
	public static bool PlayerIndentifiersEnabled { get; private set; }
	public static bool FriendlyFireEnabled { get; private set; }
	public static bool ScreenShakeEnabled { get; private set; }
	public static bool HitStopEnabled { get; private set; }

	public static float MusicVolume { get; private set; }
	public static float SoundEffectsVolume { get; private set; }

	private readonly NodePath _settingsButtonPath = new("SettingsBorder/ScrollContainer/VScrollBar");

	public override void _Ready() {
		GetNode<HSlider>(_settingsButtonPath + "/MusicVolume/HSlider").SetValueNoSignal(MusicVolume);
		GetNode<HSlider>(_settingsButtonPath + "/SoundEffectsVolume/HSlider").SetValueNoSignal(SoundEffectsVolume);
		GetNode<CheckButton>(_settingsButtonPath + "/FullscreenCheckButton").ButtonPressed = FullScreenEnabled;
		GetNode<CheckButton>(_settingsButtonPath + "/PlayerIdentifiersCheckBox").ButtonPressed = PlayerIndentifiersEnabled;
		GetNode<CheckButton>(_settingsButtonPath + "/FriendlyFireCheckButton").ButtonPressed = FriendlyFireEnabled;
		GetNode<CheckButton>(_settingsButtonPath + "/ScreenShakeCheckButton").ButtonPressed = ScreenShakeEnabled;
		GetNode<CheckButton>(_settingsButtonPath + "/HitstopCheckButton").ButtonPressed = HitStopEnabled;
	}

	public static void SetValuesFrom(SettingsFileModel settings) {
		_musicAudioBusIndex = AudioServer.GetBusIndex("Music");
		MusicVolume = settings.MusicVolume;
		SoundEffectsVolume = settings.SoundEffectsVolume;
		SetAudioBusVolume(_musicAudioBusIndex, MusicVolume);

		_soundEffectsAudioBusIndex = AudioServer.GetBusIndex("Sound Effects");
		FullScreenEnabled = settings.FullScreenEnabled;
		OnFullScreenButtonToggled(FullScreenEnabled);
		SetAudioBusVolume(_soundEffectsAudioBusIndex, SoundEffectsVolume);

		PlayerIndentifiersEnabled = settings.PlayerIndentifiersEnabled;
		FriendlyFireEnabled = settings.FriendlyFireEnabled;
		ScreenShakeEnabled = settings.ScreenShakeEnabled;
		HitStopEnabled = settings.HitStopEnabled;
	}

	public static void SaveSettings() {
		SaveController.SaveSettings(new SettingsFileModel(
			musicVolume: MusicVolume,
			soundEffectsVolume: SoundEffectsVolume,
			fullScreenEnabled: FullScreenEnabled,
			playerIndentifiersEnabled: PlayerIndentifiersEnabled,
			friendlyFireEnabled: FriendlyFireEnabled,
			screenShakeEnabled: ScreenShakeEnabled,
			hitStopEnabled: HitStopEnabled
		));
	}

	public static void OnMusicVolumeSliderValueChanged(float value) {
		MusicVolume = value;
		SetAudioBusVolume(_musicAudioBusIndex, MusicVolume);
	}

	public static void OnSoundEffectsVolumeSliderDragEnded(float value) {
		SoundEffectsVolume = value;
		SetAudioBusVolume(_soundEffectsAudioBusIndex, SoundEffectsVolume);
	}

	private static void SetAudioBusVolume(int audioBusIndex, float volume) {
		AudioServer.SetBusVolumeDb(
			busIdx: audioBusIndex,
			volumeDb: Mathf.LinearToDb(volume)
		);
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
