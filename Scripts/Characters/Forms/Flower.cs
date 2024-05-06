using Attacks;
using Attacks.Flower;
using Characters;

namespace Forms;

public partial class Flower : Form {
    public override void Attack() {
        AttackBase.Instantiate<FlowerSpores>(GetParent<Character>(), AttacksShouldAffectEnemies, AttacksShouldAffectPlayers);
        Sprite.Play("attack");

        AttackTimer.WaitTime = Attacks[0].Duration;
        AttackTimer.Start();
        CurrentState = State.Attacking;
    }

    public override void OnAttackEnded() {
        AttackTimer.Stop();
        CurrentState = State.Idle;
    }

    public override void SpecialAction() { }
    public override void OnSpecialActionEnded() {}
}
