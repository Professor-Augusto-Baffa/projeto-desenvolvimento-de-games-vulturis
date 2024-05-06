using System.Collections.Generic;
using Godot;
using Players;
using SaveSystem;

namespace SceneController;

public partial class BaseScene : Node {
	/// <summary>
	/// Child scene with the tilemap and interactable objects.
	/// </summary>
    public static Scene ActiveScene { get; private set; }

	public static List<Player> Players { get; private set; }
	public static Camera.Camera Camera { get; private set; }
	public static ProgressionController ProgressionController { get; private set; }

	[Export]
	private PackedScene _firstScene;
	[Export]
	private Vector2 _playersInitialPosition;

	public override void _Ready() {
		Players = new List<Player>() { GetNode<Player>("Player1") };
		if (GetNodeOrNull("Player2") is Player player) {
			Players.Add(player);
		}

		Camera = GetNode<Camera.Camera>("Camera2D");
		ProgressionController = GetNode<ProgressionController>("ProgressionController");

		ChangeScene(nextScene: _firstScene.Instantiate<Scene>());
		MovePlayers(Players);
	}

	public void ChangeScene(Scene nextScene) {
		ActiveScene?.Free();
		AddChild(nextScene);
		MoveChild(childNode: nextScene, toIndex: 1);
		ActiveScene = nextScene;
	}

	private void MovePlayers(List<Player> players) {
		foreach (Player player in players) {
			player.Position = _playersInitialPosition;
		}
	}
}
