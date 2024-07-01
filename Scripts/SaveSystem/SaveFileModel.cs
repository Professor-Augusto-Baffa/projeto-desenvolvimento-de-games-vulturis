#nullable enable
using Godot;
using Godot.Collections;
using Forms;
using System.Linq;

namespace SaveSystem;

public class SaveFileModel : IFileModel {
    public PackedScene? Scene { get; private set; } = null;
    public Vector2 Position { get; private set; } = Vector2.Zero;
    public Array<Form> PlayersForms { get; private set; } = new();

    public Array<string> FormsUnlocked { get; private set; } = new() { "Flower" };
    public Array<string> HealthUpgradesCollected { get; private set; } = new();
    public Array<string> AmmoUpgradesCollected { get; private set; } = new();
    public Array<string> TutorialsCompleted { get; private set; } = new();

    public SaveFileModel(
        string sceneFilePath, Vector2 position, Array<Form> playersForms,
        Array<string> formsUnlocked, Array<string> healthUpgradesCollected, Array<string> ammoUpgradesCollected, Array<string> tutorialsCompleted
    ) {
        Scene = GD.Load<PackedScene>(sceneFilePath);
        Position = position;
        PlayersForms = playersForms;

        FormsUnlocked = formsUnlocked;
        HealthUpgradesCollected = healthUpgradesCollected;
        AmmoUpgradesCollected = ammoUpgradesCollected;
        TutorialsCompleted = tutorialsCompleted;
    }

    public SaveFileModel(string json) {
        Dictionary data = (Dictionary) Json.ParseString(json);

        Scene = GD.Load<PackedScene>((string) data["scene"]);
        Position = new Vector2(
            x: ((Dictionary<string, int>) data["position"])["x"],
            y: ((Dictionary<string, int>) data["position"])["y"]
        );

        foreach (string formFilePath in (Array<string>) data["playersForms"]) {
            Form form = GD.Load<PackedScene>(formFilePath).Instantiate<Form>();
            PlayersForms.Add(form);
        }

        FormsUnlocked = (Array<string>) data["formsUnlocked"];
        HealthUpgradesCollected = (Array<string>) data["healthUpgradesCollected"];
        AmmoUpgradesCollected = (Array<string>) data["ammoUpgradesCollected"];
        TutorialsCompleted = (Array<string>) data["tutorialsCompleted"];
    }

    /// <summary> Save data constructor with default values. </summary>
    public SaveFileModel() { }

    public string ToJson() => Json.Stringify(
        new Dictionary() {
            { "scene", Scene!.ResourcePath },
            { "position", new Dictionary() { { "x", Position.X }, { "y", Position.Y } } },
            { "playersForms", new Array<string>(PlayersForms.Select(form => form.SceneFilePath)) },
            { "formsUnlocked", FormsUnlocked },
            { "healthUpgradesCollected", HealthUpgradesCollected },
            { "ammoUpgradesCollected", AmmoUpgradesCollected },
            { "tutorialsCompleted", TutorialsCompleted },
        }
    );
}
