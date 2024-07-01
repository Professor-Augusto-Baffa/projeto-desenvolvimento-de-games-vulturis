using Godot;
using Godot.Collections;

namespace SaveSystem;

public class SettingsFileModel : IFileModel {
    public bool FullScreenEnabled { get; private set; } = false;
	public bool PlayerIndentifiersEnabled { get; private set; } = true;
	public bool FriendlyFireEnabled { get; private set; } = false;
	public bool ScreenShakeEnabled { get; private set; } = true;
	public bool HitStopEnabled { get; private set; } = true;

    public SettingsFileModel(
        bool fullScreenEnabled, bool playerIndentifiersEnabled, bool friendlyFireEnabled,
        bool screenShakeEnabled, bool hitStopEnabled
    ) {
        FullScreenEnabled = fullScreenEnabled;
        PlayerIndentifiersEnabled = playerIndentifiersEnabled;
        FriendlyFireEnabled = friendlyFireEnabled;
        ScreenShakeEnabled = screenShakeEnabled;
        HitStopEnabled = hitStopEnabled;
    }

    public SettingsFileModel(string json) {
        Dictionary data = (Dictionary) Json.ParseString(json);

        FullScreenEnabled = (bool) data["fullScreenEnabled"];
        PlayerIndentifiersEnabled = (bool) data["playerIndentifiersEnabled"];
        FriendlyFireEnabled = (bool) data["friendlyFireEnabled"];
        ScreenShakeEnabled = (bool) data["screenShakeEnabled"];
        HitStopEnabled = (bool) data["hitStopEnabled"];
    }

    public string ToJson() => Json.Stringify(
        new Dictionary() {
            { "fullScreenEnabled", FullScreenEnabled },
            { "playerIndentifiersEnabled", PlayerIndentifiersEnabled },
            { "friendlyFireEnabled", FriendlyFireEnabled },
            { "screenShakeEnabled", ScreenShakeEnabled },
            { "hitStopEnabled", HitStopEnabled },
        }
    );
}
