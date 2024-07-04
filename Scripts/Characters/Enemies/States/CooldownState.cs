using Godot;

namespace Enemies.States;

public abstract partial class CooldownState : EnemyState {
    private Timer _cooldownTimer;

    public override void _Ready() {
        base._Ready();
        _cooldownTimer = Enemy.GetNode<Timer>("CooldownBehaviour/Timer");
    }

    public override void OnStart() {
        _cooldownTimer.Start();
    }

    public override void OnEnd() {
        _cooldownTimer.Stop();
		Enemy.EmitSignal(Enemy.SignalName.NewTargetNeeded);
    }

    public override void Process(float delta) { }
}
