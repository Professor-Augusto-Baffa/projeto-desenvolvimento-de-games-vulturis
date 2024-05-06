using Godot;
using Players;
using SceneController;

namespace Objects;

public partial class AmmoUpgrade : Area2D {
	public override void _Ready() {
		if (BaseScene.ProgressionController.CollectedAmmoUpgradeIn(GetParent().Name)) {
			QueueFree();
		}
	}

	public void OnBodyEntered(Node body) {
		if (body is not Player) return;
		BaseScene.ProgressionController.AmmoUpgradesCollected.Add(GetParent().Name);
		foreach (Player player in BaseScene.Players) {
			player.RestoreAmmo();
		}
		QueueFree();
	}
}
