using Godot;
using Godot.Collections;

namespace SaveSystem;

public class SettingsFileModel : IFileModel {
    public float MusicVolume { get; private set; } = 1;
    public float SoundEffectsVolume { get; private set; } = 1;
    public bool FullScreenEnabled { get; private set; } = true;
	public bool PlayerIndentifiersEnabled { get; private set; } = true;
	public bool FriendlyFireEnabled { get; private set; } = false;
	public bool ScreenShakeEnabled { get; private set; } = true;
	public bool HitStopEnabled { get; private set; } = true;

    public SettingsFileModel(
        float musicVolume, float soundEffectsVolume,
        bool fullScreenEnabled, bool playerIndentifiersEnabled, bool friendlyFireEnabled,
        bool screenShakeEnabled, bool hitStopEnabled
    ) {
        MusicVolume = musicVolume;
        SoundEffectsVolume = soundEffectsVolume;
        FullScreenEnabled = fullScreenEnabled;
        PlayerIndentifiersEnabled = playerIndentifiersEnabled;
        FriendlyFireEnabled = friendlyFireEnabled;
        ScreenShakeEnabled = screenShakeEnabled;
        HitStopEnabled = hitStopEnabled;
    }

    public SettingsFileModel(string json) {
        Dictionary data = (Dictionary) Json.ParseString(json);

        MusicVolume = (float) data["musicVolume"];
        SoundEffectsVolume = (float) data["soundEffectsVolume"];
        FullScreenEnabled = (bool) data["fullScreenEnabled"];
        PlayerIndentifiersEnabled = (bool) data["playerIndentifiersEnabled"];
        FriendlyFireEnabled = (bool) data["friendlyFireEnabled"];
        ScreenShakeEnabled = (bool) data["screenShakeEnabled"];
        HitStopEnabled = (bool) data["hitStopEnabled"];
    }

    /// <summary> Settings data constructor with default values. </summary>
    public SettingsFileModel() {}

    public string ToJson() => Json.Stringify(
        new Dictionary() {
            { "musicVolume", MusicVolume },
            { "soundEffectsVolume", SoundEffectsVolume },
            { "fullScreenEnabled", FullScreenEnabled },
            { "playerIndentifiersEnabled", PlayerIndentifiersEnabled },
            { "friendlyFireEnabled", FriendlyFireEnabled },
            { "screenShakeEnabled", ScreenShakeEnabled },
            { "hitStopEnabled", HitStopEnabled },
        }
    );
}
