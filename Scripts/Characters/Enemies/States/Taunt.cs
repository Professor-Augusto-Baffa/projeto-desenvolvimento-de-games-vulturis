using Godot;

namespace Enemies.States;

/// <summary> Taunt behaviour in which the enemy goes back and forth for the Timer duration. </summary>
public partial class Taunt : CooldownState {
    private Timer _directionChangeTimer;

    [Export]
    private float _speedMultiplier;

    private Vector2 _walkDirection;

    public override void _Ready() {
        base._Ready();
        _directionChangeTimer = GetNode<Timer>("DirectionChangeTimer");
    }

    public override void OnStart() {
        base.OnStart();
        _directionChangeTimer.Start();

        if (Enemy.Target is null) {
            _walkDirection = Vector2.Left;
        } else {
            _walkDirection = Enemy.TargetDirection;
        }
    }

    public override void Process(float delta) {
        base.Process(delta);
        Enemy.Walk(delta, _walkDirection, _speedMultiplier);
    }

    public override void OnEnd() {
        base.OnEnd();
        _directionChangeTimer.Stop();
    }

    public void ChangeDirections() {
        _walkDirection = -_walkDirection;
    }
}
