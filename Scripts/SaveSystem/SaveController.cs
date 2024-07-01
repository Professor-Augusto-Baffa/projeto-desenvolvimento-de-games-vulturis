using Godot;

namespace SaveSystem;

public partial class SaveController : Node {
    private const string _savePath = "user://vulturis_save.sav";
    private const string _settingsPath = "user://vulturis_settings.sav";

    public static void Save(SaveFileModel data) => SaveFile(_savePath, data);
    public static void SaveSettings(SettingsFileModel data) => SaveFile(_settingsPath, data);

    private static void SaveFile(string path, IFileModel data) {
        using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file.StoreLine(data.ToJson());
    }

    #nullable enable
    public static SaveFileModel? Load() {
        if (!FileAccess.FileExists(_savePath)) return null;

        using FileAccess file = FileAccess.Open(_savePath, FileAccess.ModeFlags.Read);
        return new SaveFileModel(json: file.GetLine());
    }

    public static SettingsFileModel? LoadSettings() {
        if (!FileAccess.FileExists(_settingsPath)) return null;

        using FileAccess file = FileAccess.Open(_settingsPath, FileAccess.ModeFlags.Read);
        return new SettingsFileModel(json: file.GetLine());
    }
}
