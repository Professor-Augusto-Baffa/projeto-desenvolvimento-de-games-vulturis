using System.Collections.Generic;
using Godot;
using Players;
using SaveSystem;
using UI;
using Menus.Settings;

namespace SceneController;

/// <summary>
/// Base Scene containing startup, scene transitions, and static references to commonly used nodes.
/// </summary>
/// <remarks>
/// Don't try to access static properties in _Ready(). They only get set in this node's _Ready(), which only gets called after all children are instanced.
/// </remarks>
public partial class BaseScene : Node {
	private static readonly PackedScene _prefab = GD.Load<PackedScene>("res://Scenes/Base.tscn");
	private static readonly PackedScene _multiplayerPrefab = GD.Load<PackedScene>("res://Scenes/Multiplayer Base.tscn");

	/// <summary> Child scene with the tilemap and interactable objects. </summary>
    public static Scene ActiveScene { get; private set; }

	public static List<Player> Players { get; private set; }
	public static Camera.Camera Camera { get; private set; }
	public static ProgressionController ProgressionController { get; private set; }
	public static CanvasLayer UICanvas { get; private set; }

	private AudioStreamPlayer _musicPlayer;

	[Export]
	private PackedScene _firstScene;
	[Export]
	private Vector2 _playersInitialPosition;

	public static BaseScene Instantiate(SaveFileModel save, bool isMultiplayer) {
		BaseScene scene = isMultiplayer ? _multiplayerPrefab.Instantiate<BaseScene>() : _prefab.Instantiate<BaseScene>();

		if (save.Scene != null) scene._firstScene = save.Scene;
		scene._playersInitialPosition = save.Position;

		scene.LoadPlayers();
		int index = 0;
		foreach (Player player in Players) {
			if (index < save.PlayersForms.Count) {
				player.NextForm = save.PlayersForms[index];
				// There's no need to call UpdateForm(), as it is called during the player's _Ready().
			}
			// There's no need to change forms for players not included in the save.PlayersForms list, as they will use the prefab's default form.
			index++;
		}

		ProgressionController = scene.GetNode<ProgressionController>("ProgressionController");
		ProgressionController.SetValuesFrom(save);

		return scene;
	}

	public override void _Ready() {
		LoadPlayers();
		Camera = GetNode<Camera.Camera>("Camera2D");
		ProgressionController = GetNode<ProgressionController>("ProgressionController");
		UICanvas = GetNode<CanvasLayer>("CanvasLayer");
		_musicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");

		ChangeScene(_firstScene.Instantiate<Scene>());
		MovePlayersTo(position: _playersInitialPosition, shouldAddToCurrentPosition: true);
		ChangePlayerIndicatorVisibility(Settings.PlayerIndentifiersEnabled);
	}

	public async void ChangeScene(Scene nextScene) {
		if (IsInstanceValid(ActiveScene)) ActiveScene.Free();
		AddChild(nextScene);
		MoveChild(childNode: nextScene, toIndex: 1);
		ActiveScene = nextScene;

		if (nextScene.BackgroundMusic != null) ChangeMusic(nextScene.BackgroundMusic);
		SceneTransition.FadeOut();

		Camera.PositionSmoothingEnabled = false;
		await ToSignal(
			source: GetTree().CreateTimer(.1f),
			signal: SceneTreeTimer.SignalName.Timeout
		);
		Camera.PositionSmoothingEnabled = true;
	}

	public static async void MovePlayersTo(Vector2 position, bool shouldAddToCurrentPosition = false) {
		foreach (Player player in Players) {
			CollisionShape2D collisionShape = player.GetNode<CollisionShape2D>("CollisionShape2D");
			collisionShape.Disabled = true;

			if (shouldAddToCurrentPosition) {
				player.Position += position;
			} else {
				player.Position = position;
			}

			await player.ToSignal(source: player.GetTree(), signal: SceneTree.SignalName.ProcessFrame);
			collisionShape.Disabled = false;
		}
	}

	public static void ChangePlayerIndicatorVisibility(bool visibility) {
		foreach (Player player in Players) {
			player.GetNode<Sprite2D>("PlayerIndicator").Visible = visibility;
		}
	}

	private void LoadPlayers() {
		Players = new List<Player>() { GetNode<Player>("Player1") };
		if (GetNodeOrNull("Player2") is Player player) {
			Players.Add(player);
		}
	}

	private void ChangeMusic(AudioStream music) {
		if (_musicPlayer.Stream == music) return;
		_musicPlayer.Stream = music;
		_musicPlayer.Play();
	}
}
