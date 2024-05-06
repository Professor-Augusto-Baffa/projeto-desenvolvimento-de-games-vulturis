using Godot;

namespace Objects;

public partial class Door : StaticBody2D {
	[Export]
	private float _openDuration = 1;

	public bool IsOpen { get; private set; }

	public async void Open() {
		GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play();
		IsOpen = true;

		await ToSignal(
			source: GetTree().CreateTimer(_openDuration),
			signal: SceneTreeTimer.SignalName.Timeout
		);
		CollisionShape2D collision = GetNode<CollisionShape2D>("CollisionShape2D");
		collision.Disabled = true;
	}
}
