using Godot;
using SaveSystem;
using SceneController;

namespace Menus.MainMenu;

public partial class MainMenu : Control {
	[Export]
	private AudioStream _music;

	private static bool _loadedSettingsForTheFirstTime = false;

	public override void _Ready() {
		if (GetNodeOrNull<AudioStreamPlayer>("MusicPlayer") is not null) return;

		AudioStreamPlayer musicPlayer = new() {
			Name = "MusicPlayer",
			Stream = _music,
			Bus = "Music",
			ProcessMode = ProcessModeEnum.Always
		};
		AddChild(musicPlayer);
		musicPlayer.Play();

		if (!_loadedSettingsForTheFirstTime) LoadSettings();
	}

	public void OnNewGameButtonPressed() => StartGame(this, shouldLoadSave: false);
	public void OnContinueButtonPressed() => StartGame(this);

	#nullable enable
	public static void StartGame(Node rootNode, bool shouldLoadSave = true) {
		SaveFileModel? save = null;
		if (shouldLoadSave) save = SaveController.Load();
		save ??= new();

		rootNode.QueueFree();
		BaseScene scene = BaseScene.Instantiate(save, MultiplayerCheckButton.IsMultiplayerActive);
		rootNode.GetParent().AddChild(scene);
	}

	public void OnExitButtonPressed() {
		GetTree().Quit();
	}

	private static void LoadSettings() {
		SettingsFileModel settings = SaveController.LoadSettings() ?? new();
        Settings.Settings.SetValuesFrom(settings);
		_loadedSettingsForTheFirstTime = true;
	}
}
