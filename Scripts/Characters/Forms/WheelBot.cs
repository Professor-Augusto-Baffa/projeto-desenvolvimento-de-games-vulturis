using Godot;
using Attacks;
using Attacks.WheelBot;
using Characters;

namespace Forms;

public partial class WheelBot : Form {
    [Export]
    private int _fireDashSpeed;

    public override void Attack() {
		AttackTimer.WaitTime = Attacks[0].Duration;
		AttackTimer.Start();
		AttackDelayTimer.WaitTime = Attacks[0].StartingDelay;
		AttackDelayTimer.Start();

        Sprite.Play("attack");
		CurrentState = State.Attacking;
    }

    public void OnAttackDelayEnded() {
		AttackDelayTimer.Stop();
        Character parent = GetParent<Character>();
        AttackBase.Instantiate<WheelBotShot>(parent, AttacksShouldAffectEnemies, AttacksShouldAffectPlayers, parent.Position);
    }

    public override void OnAttackEnded() {
		AttackTimer.Stop();
		CurrentState = State.Idle;
    }

    public override void SpecialAction() {
        Character parent = GetParent<Character>();
        AttackBase.Instantiate<WheelBotFireDash>(parent, AttacksShouldAffectEnemies, AttacksShouldAffectPlayers, parent.Position);
        Sprite.Play("special");

    	float deltaTime = (float) GetProcessDeltaTime() * 60;
        parent.Velocity = (Sprite.FlipH ? Vector2.Left : Vector2.Right) * _fireDashSpeed / deltaTime;
        parent.MoveAndSlide();

        SpecialActionTimer.WaitTime = FormStats.SpecialActionDuration;
        SpecialActionTimer.Start();

        CurrentState = State.UsingSpecialAction;
    }

    public override void OnSpecialActionEnded() {
        SpecialActionTimer.Stop();
        CurrentState = State.Idle;
    }
}
