using Godot;
using Players;
using SceneController;

namespace Objects;

public partial class HealthUpgrade : Area2D {
	public override void _Ready() {
		if (BaseScene.ProgressionController.CollectedHealthUpgradeIn(GetParent().Name)) {
			QueueFree();
		}
	}

	public void OnBodyEntered(Node body) {
		if (body is not Player) return;
		BaseScene.ProgressionController.HealthUpgradesCollected.Add(GetParent().Name);
		foreach (Player player in BaseScene.Players) {
			player.Heal();
		}
		QueueFree();
	}
}
