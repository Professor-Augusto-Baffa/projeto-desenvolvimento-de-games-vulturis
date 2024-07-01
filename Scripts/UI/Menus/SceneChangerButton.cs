using Godot;

namespace Menus;

public partial class SceneChangerButton : BaseButton {
	[Export(PropertyHint.File, "*.tscn")]
	private string _scenePath;

	[Export]
	private bool _shouldKeepMusic = true;

	private PackedScene _scenePrefab;

	public override void _Ready() {
		_scenePrefab = GD.Load<PackedScene>(_scenePath);
	}

	public void OnPressed() => ChangeScene();

	protected virtual void ChangeScene() {
		SceneTree tree = GetTree();
		Node previousScene = tree.Root.GetChild(0);
		Node nextScene = _scenePrefab.Instantiate();

		if (_shouldKeepMusic) {
			AudioStreamPlayer musicPlayer = previousScene.GetNode<AudioStreamPlayer>("MusicPlayer");
			previousScene.RemoveChild(musicPlayer);
			nextScene.AddChild(musicPlayer);
		}

		previousScene.QueueFree();
		tree.Root.AddChild(nextScene);
	}
}
