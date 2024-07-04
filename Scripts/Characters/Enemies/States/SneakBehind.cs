using Godot;

namespace Enemies.States;

public partial class SneakBehind : CooldownState {
	/// <summary> Distance when the enemy gets close enough to the target. </summary>
	[Export]
	private float _distanceToTarget;

	private Vector2 _destination;

    public override void OnStart() {
        base.OnStart();
		if (Enemy.Target is null) {
			_destination = Enemy.Position;
			return;
		}

		_destination = Enemy.Target.Position;

		if (Enemy.TargetDirection == Vector2.Left) {
			_destination.X -= _distanceToTarget;
		} else {
			_destination.X += _distanceToTarget;
		}
    }

    public override void Process(float delta) {
        base.Process(delta);
		Vector2 direction = Vector2.Zero;

		if (Enemy.Position.DistanceTo(_destination) > 1) {
			if (_destination.X < Enemy.Position.X) {
				direction = Vector2.Left;
			} else if (_destination.X > Enemy.Position.X) {
				direction = Vector2.Right;
			}
		}

		Enemy.Walk(delta, direction);
    }
}
