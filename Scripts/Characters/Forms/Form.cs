using Godot;
using Attacks;
using Godot.Collections;

namespace Forms;

public abstract partial class Form : Node2D {
	[Export]
	public FormStats FormStats { get; private set; }

	public string FormName => GetType().Name;

	protected Array<AttackStats> Attacks => FormStats.Attacks;

	public enum State { Idle, Attacking, UsingSpecialAction }
	public State CurrentState { get; protected set; } = State.Idle;

	public virtual bool CanAttack => CurrentState == State.Idle;
	public abstract void Attack();
	public abstract void OnAttackEnded();

	public virtual bool CanUseSpecialAction => CurrentState == State.Idle;
	public abstract void SpecialAction();
	public abstract void OnSpecialActionEnded();

	protected bool AttacksShouldAffectEnemies { get; private set; } = false;
	protected bool AttacksShouldAffectPlayers { get; private set; } = true;

	public void UpdateAttackCollisions(bool shouldAffectEnemies, bool shouldAffectPlayers) {
		AttacksShouldAffectEnemies = shouldAffectEnemies;
		AttacksShouldAffectPlayers = shouldAffectPlayers;
	}

	public AnimatedSprite2D Sprite { get; private set; }
	protected Timer AttackTimer { get; private set; }
	protected Timer SpecialActionTimer { get; private set; }
	protected Timer AttackDelayTimer { get; private set; }

	public override void _Ready() {
		Sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		AttackTimer = GetNode<Timer>("AttackTimer");
		SpecialActionTimer = GetNode<Timer>("SpecialActionTimer");
		AttackDelayTimer = GetNode<Timer>("AttackDelayTimer");
	}
}
