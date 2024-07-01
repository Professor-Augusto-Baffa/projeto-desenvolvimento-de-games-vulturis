using Godot;
using Attacks;
using Attacks.Assassin;
using Characters;

namespace Forms;

public partial class ShockSweeper : Form {
	private Timer _specialActionDelayTimer;

	[Export]
	private float _sweepDelayDuration;
	[Export]
	private float _sweepSpeed;

	/// <summary> Correlation between FormStats Attacks array indexes and attack names. </summary>
	private enum AttackType { Slam = 0, SpinSlam = 1 }

    public override bool CanAttack => base.CanAttack || (CurrentState == State.UsingSpecialAction && !_shouldStartSpinSlam);

    private bool _shouldStartSpinSlam = false;

    public override void _Ready() {
        base._Ready();
		_specialActionDelayTimer = GetNode<Timer>("SpecialActionDelayTimer");
    }

    public override void Attack() {
        if (CurrentState == State.UsingSpecialAction) {
            _shouldStartSpinSlam = true;
            return;
        }

        AttackTimer.WaitTime = Attacks[(int) AttackType.Slam].Duration;
        AttackTimer.Start();
        AttackDelayTimer.WaitTime = Attacks[(int) AttackType.Slam].StartingDelay;
        AttackDelayTimer.Start();

        Sprite.Play("attack");
        CurrentState = State.Attacking;
    }

    public void OnAttackDelayEnded() {
        CurrentAttack = AttackBase.Instantiate<ShockSweeperSlam>(GetParent<Character>(), AttacksShouldAffectEnemies, AttacksShouldAffectPlayers);
    }

    public override void StopAttack() {
        _shouldStartSpinSlam = false;
        AttackDelayTimer.Stop();
        base.StopAttack();
    }

    public override void SpecialAction() {
        SpecialActionTimer.WaitTime = FormStats.SpecialActionDuration;
        SpecialActionTimer.Start();
        _specialActionDelayTimer.WaitTime = _sweepDelayDuration;
        _specialActionDelayTimer.Start();

        Sprite.Play("special");
        CurrentState = State.UsingSpecialAction;
    }

    public void OnSpecialActionDelayEnded() {
        _specialActionDelayTimer.Stop();
        Character parent = GetParent<Character>();

        AttackBase.Instantiate<ShockSweeperSweep>(parent, AttacksShouldAffectEnemies, AttacksShouldAffectPlayers, parent.Position);
		float deltaTime = (float) GetProcessDeltaTime() * 60;
        parent.Velocity = (Sprite.FlipH ? Vector2.Left : Vector2.Right) * _sweepSpeed / deltaTime;
        parent.MoveAndSlide();
    }

    public override void OnSpecialActionEnded() {
        base.OnSpecialActionEnded();

        if (_shouldStartSpinSlam) {
            CurrentAttack = AttackBase.Instantiate<ShockSweeperSpinSlam>(GetParent<Character>(), AttacksShouldAffectEnemies, AttacksShouldAffectPlayers);
            Sprite.Play("spin_slam");

            AttackTimer.WaitTime = Attacks[(int) AttackType.SpinSlam].Duration;
            AttackTimer.Start();

            _shouldStartSpinSlam = false;
            CurrentState = State.Attacking;
        }
    }
}
