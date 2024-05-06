using Godot;
using Godot.Collections;
using Players;

namespace Camera;

public partial class CameraTarget : RemoteTransform2D {
	[Export]
    private Vector2 _defaultOffset = Vector2.Zero;
    [Export]
    private float _movementDirectionOffset;

    [Export]
    private Array<Player> _players = new();

    private Vector2 _playersMovementDirection = Vector2.Zero;

    public override void _PhysicsProcess(double delta) {
        Position = _defaultOffset;

        Vector2 movementDirection = GetPlayersMovementDirection();
        if (movementDirection != Vector2.Zero) {
            _playersMovementDirection = movementDirection;
        }
        Position += _playersMovementDirection * _movementDirectionOffset;

        foreach (Player player in _players) {
            Position += player.Position;
        }
        Position /= _players.Count;
    }

    private Vector2 GetPlayersMovementDirection() {
        float horizontalDirection = 0;
        foreach (Player player in _players) {
            if (player.Velocity.X != 0) {
                horizontalDirection += player.Velocity.X;
            }
        }
        return new Vector2(horizontalDirection, 0).Normalized();
    }
}
