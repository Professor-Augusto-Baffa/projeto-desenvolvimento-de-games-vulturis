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

	public async void OnBodyEntered(Node body) {
		if (body is not Player) return;
		BaseScene.ProgressionController.AmmoUpgradesCollected.Add(GetParent().Name);
		foreach (Player player in BaseScene.Players) {
			player.RestoreAmmo();
		}

		// Temporarilty disable this node to play the sound effect, before destroying it
		GetNode<AnimatedSprite2D>("AnimatedSprite2D").Visible = false;
		CollisionMask = 0;

		await PlaySoundEffect();
		QueueFree();
	}

	private SignalAwaiter PlaySoundEffect() {
		AudioStreamPlayer2D audioPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		audioPlayer.Play();
		return ToSignal(source: audioPlayer, signal: AudioStreamPlayer2D.SignalName.Finished);
	}
}
