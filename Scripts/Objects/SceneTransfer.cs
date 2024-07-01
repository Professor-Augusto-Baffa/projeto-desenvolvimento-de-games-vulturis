using System.Collections.Generic;
using Godot;
using SceneController;
using Players;

namespace Objects;

public partial class SceneTransfer : Area2D {
	[Export(PropertyHint.File, "*.tscn")]
	private string _nextScene;
	// Scenes cannot be directly passed in an Export field because of circular imports between scenes
	private PackedScene _nextScenePrefab;

	[Export]
	private Vector2 _positionInNextScene;

	public override async void _Ready() {
		_nextScenePrefab = GD.Load<PackedScene>(_nextScene);
		GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
		await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
		GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
	}

	private void OnBodyEntered(Node2D node) => CallDeferred(nameof(InstantiateNextScene)); // Changing scenes should only happen after physics calculations

	private void InstantiateNextScene() {
		BaseScene baseScene = GetNode<BaseScene>("../../");
		baseScene.ChangeScene(nextScene: _nextScenePrefab.Instantiate<Scene>());
		MovePlayers(BaseScene.Players);
	}

	private void MovePlayers(List<Player> players) {
		foreach (Player player in players) {
			player.Position = _positionInNextScene;
		}
	}
}
