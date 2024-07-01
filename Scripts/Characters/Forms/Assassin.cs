using Godot;
using Attacks;
using Attacks.Assassin;
using Characters;

namespace Forms;

public partial class Assassin : Form {
	[Export]
	private float _sweepDelayDuration;
	[Export]
	private float _sweepSpeed;

	/// <summary>
	/// Correlation between FormStats Attacks array indexes and attack names.
	/// </summary>
	private enum AttackType { FirstSlash = 0, SecondSlash = 1, CrossSlice = 2 }

    public override bool CanAttack => _nextAttack < AttackType.SecondSlash;

	private AttackType _nextAttack = AttackType.FirstSlash;
	private bool _shouldStartAnotherAttack = false;

	private Timer _specialActionDelayTimer;

    public override void _Ready() {
        base._Ready();
		_specialActionDelayTimer = GetNode<Timer>("SpecialActionDelayTimer");
    }

    public override void Attack() {
		if (CurrentState == State.Idle) {
			CurrentAttack = AttackBase.Instantiate<AssassinSlashOne>(GetParent<Character>(), AttacksShouldAffectEnemies, AttacksShouldAffectPlayers);
			Sprite.Play("attack");

			AttackTimer.WaitTime = Attacks[(int) _nextAttack].Duration;
			AttackTimer.Start();
			CurrentState = State.Attacking;

		} else {
			_shouldStartAnotherAttack = true;
			if (CurrentState == State.Attacking) _nextAttack = AttackType.SecondSlash;
			if (CurrentState == State.UsingSpecialAction) _nextAttack = AttackType.CrossSlice;
		}
    }

    public override void OnAttackEnded() {
		if (_shouldStartAnotherAttack && _nextAttack == AttackType.SecondSlash) {
			CurrentAttack = AttackBase.Instantiate<AssassinSlashTwo>(GetParent<Character>(), AttacksShouldAffectEnemies, AttacksShouldAffectPlayers);
			Sprite.Play("attack_2");

			AttackTimer.WaitTime = Attacks[(int) _nextAttack].Duration;
			AttackTimer.Start();
			_nextAttack++;
		} else {
			AttackTimer.Stop();
			_nextAttack = AttackType.FirstSlash;
			base.OnAttackEnded();
		}
		_shouldStartAnotherAttack = false;
	}

    public override void StopAttack() {
		_shouldStartAnotherAttack = false;
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

		AttackBase.Instantiate<AssassinSweep>(parent, AttacksShouldAffectEnemies, AttacksShouldAffectPlayers, parent.Position);
		float deltaTime = (float) GetProcessDeltaTime() * 60;
        parent.Velocity = (Sprite.FlipH ? Vector2.Left : Vector2.Right) * _sweepSpeed / deltaTime;
        parent.MoveAndSlide();
	}

    public override void OnSpecialActionEnded() {
		base.OnSpecialActionEnded();

		if (_shouldStartAnotherAttack) {
			CurrentAttack = AttackBase.Instantiate<AssassinCrossSlice>(GetParent<Character>(), AttacksShouldAffectEnemies, AttacksShouldAffectPlayers);
			Sprite.Play("cross_slice");

			_nextAttack = AttackType.CrossSlice;
			AttackTimer.WaitTime = Attacks[(int) _nextAttack].Duration;
			AttackTimer.Start();

			CurrentState = State.Attacking;
		}
    }
}
