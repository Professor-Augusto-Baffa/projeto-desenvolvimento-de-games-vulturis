using Godot;
using Godot.Collections;
using SaveSystem;
using SceneController;
using Forms;
using Players;

namespace Objects;

public partial class Panel : Area2D {
    private AnimatedSprite2D _sprite;

    public bool IsBeingUsed { get; private set; } = false;

    public override void _Ready() {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

    public void Save() {
        if (IsBeingUsed) return;

        IsBeingUsed = true;
        _sprite.Play("activate");

        SaveController.Save(new SaveFileModel(
            sceneFilePath: BaseScene.ActiveScene.SceneFilePath,
            position: Position,
            playersForms: new Array<Form>(BaseScene.Players.ConvertAll(player => player.Form)),
            formsUnlocked: BaseScene.ProgressionController.FormsUnlocked,
            healthUpgradesCollected: new Array<string>(BaseScene.ProgressionController.HealthUpgradesCollected),
            ammoUpgradesCollected: new Array<string>(BaseScene.ProgressionController.AmmoUpgradesCollected),
            tutorialsCompleted: new Array<string>(BaseScene.ProgressionController.TutorialsCompleted)
        ));

        foreach (Player player in BaseScene.Players) {
            player.Heal();
            player.AddAmmo();
        }
    }

    public void OnAnimationFinished() {
        if (!IsBeingUsed) return;
        _sprite.Play("deactivate");
        IsBeingUsed = false;
    }
}
