using Attacks;
using Attacks.BlindingSpider;
using Characters;

namespace Forms;

public partial class BlindingSpider : Form {
    public override void Attack() {
		AttackTimer.WaitTime = Attacks[0].Duration;
		AttackTimer.Start();
		AttackDelayTimer.WaitTime = Attacks[0].StartingDelay;
		AttackDelayTimer.Start();

		Sprite.Play("attack");
		CurrentState = State.Attacking;
    }

	public void OnAttackDelayEnded() {
		CurrentAttack = AttackBase.Instantiate<BlindingSpiderBlind>(GetParent<Character>(), AttacksShouldAffectEnemies, AttacksShouldAffectPlayers);
	}

    public override void StopAttack() {
		AttackDelayTimer.Stop();
        base.StopAttack();
    }

    public override void SpecialAction() { }
    public override void OnSpecialActionEnded() { }
}
