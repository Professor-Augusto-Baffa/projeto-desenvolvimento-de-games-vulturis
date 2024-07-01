using Attacks;
using Attacks.ShotgunnerBot;
using Characters;

namespace Forms;

public partial class ShotgunnerBot : Form {
    public override void Attack() {
        CurrentAttack = AttackBase.Instantiate<ShotgunnerBotBashOne>(GetParent<Character>(), AttacksShouldAffectEnemies, AttacksShouldAffectPlayers);
        Sprite.Play("attack");

        AttackTimer.WaitTime = Attacks[0].Duration + Attacks[0].StartingDelay;
        AttackTimer.Start();
        AttackDelayTimer.WaitTime = Attacks[0].StartingDelay; // delay to instantiate the second bash
        AttackDelayTimer.Start();

        CurrentState = State.Attacking;
    }

    public void OnAttackDelayEnded() {
        CurrentAttack = AttackBase.Instantiate<ShotgunnerBotBashTwo>(GetParent<Character>(), AttacksShouldAffectEnemies, AttacksShouldAffectPlayers);
    }

    public override void StopAttack() {
        AttackDelayTimer.Stop();
        base.StopAttack();
    }

    public override void SpecialAction() {
        AttackBase.Instantiate<ShotgunnerBotShot>(GetParent<Character>(), AttacksShouldAffectEnemies, AttacksShouldAffectPlayers);
        Sprite.Play("special");

        SpecialActionTimer.WaitTime = FormStats.SpecialActionDuration;
        SpecialActionTimer.Start();

        CurrentState = State.UsingSpecialAction;
    }
}
