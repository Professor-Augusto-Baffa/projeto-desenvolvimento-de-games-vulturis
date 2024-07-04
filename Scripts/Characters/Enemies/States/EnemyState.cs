using Godot;

namespace Enemies.States;

/// <summary> Base class for defining not generic enemy behaviours. </summary>
public abstract partial class EnemyState : Node {
    protected Enemy Enemy;

    public override void _Ready() {
        Enemy = GetParent<Enemy>();
    }

    public abstract void OnStart();
    public abstract void Process(float delta);
    public abstract void OnEnd();
}
