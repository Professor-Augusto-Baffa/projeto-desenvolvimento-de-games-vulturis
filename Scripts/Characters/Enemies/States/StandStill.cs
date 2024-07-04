namespace Enemies.States;

public partial class StandStill : CooldownState {
    public override void OnStart() {
        base.OnStart();
		Enemy.Sprite.Play("idle");
    }

    public override void Process(float delta) {
        base.Process(delta);
        Enemy.Fall(delta);
    }
}
