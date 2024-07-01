using Godot;
using SceneController;

namespace Objects;

public partial class SceneTransfer : Area2D {
	[Export(PropertyHint.File, "*.tscn")]
	private string _nextScene;
	// Scenes cannot be directly passed in an Export field because of circular imports between scenes
	private PackedScene _nextScenePrefab;

	[Export]
	private Vector2 _positionInNextScene;

	public override void _Ready() {
		_nextScenePrefab = GD.Load<PackedScene>(_nextScene);
	}

	private void OnBodyEntered(Node2D node) => CallDeferred(nameof(InstantiateNextScene)); // Changing scenes should only happen after physics calculations

	private void InstantiateNextScene() {
		BaseScene baseScene = GetNode<BaseScene>("../../");
		baseScene.ChangeScene(nextScene: _nextScenePrefab.Instantiate<Scene>());
		BaseScene.MovePlayersTo(_positionInNextScene);
	}
}
