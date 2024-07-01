using Godot;
using Attacks;
using Godot.Collections;

namespace Forms;

public abstract partial class Form : Node2D {
	public AnimatedSprite2D Sprite { get; private set; }
	protected Timer AttackTimer { get; private set; }
	protected Timer SpecialActionTimer { get; private set; }
	protected Timer AttackDelayTimer { get; private set; }

	[Export]
	public FormStats FormStats { get; private set; }

	[ExportGroup("Sound Effects")]
	[Export] public AudioStream DamageSound { get; private set; }
	[Export] public AudioStream DeathSound { get; private set; }

	public string FormName => GetType().Name;

	protected Array<AttackStats> Attacks => FormStats.Attacks;

	public enum State { Idle, Attacking, UsingSpecialAction }
	public State CurrentState { get; protected set; } = State.Idle;

	public virtual bool CanAttack => CurrentState == State.Idle;
	public abstract void Attack();
    public virtual void OnAttackEnded() {
		CurrentState = State.Idle;
		CurrentAttack = null;
	}

	public virtual bool CanUseSpecialAction => CurrentState == State.Idle;
	public abstract void SpecialAction();
	public virtual void OnSpecialActionEnded() => CurrentState = State.Idle;

	#nullable enable
	/// <summary>
	/// The AttackBase that might be destroyed if the attack is stopped.
	/// Don't associate this with attacks that are not supposed to be destroyed.
	/// </summary>
	protected AttackBase? CurrentAttack;

	public virtual void StopAttack() {
		if (!IsInstanceValid(CurrentAttack)) CurrentAttack = null; // Removes already destroyed attack
		CurrentAttack?.QueueFree();
		CurrentAttack = null;
		AttackTimer.Stop();
		OnAttackEnded();
	}

	protected bool AttacksShouldAffectEnemies { get; private set; } = false;
	protected bool AttacksShouldAffectPlayers { get; private set; } = true;

	public void UpdateAttackCollisions(bool shouldAffectEnemies, bool shouldAffectPlayers) {
		AttacksShouldAffectEnemies = shouldAffectEnemies;
		AttacksShouldAffectPlayers = shouldAffectPlayers;
	}

	public override void _Ready() {
		Sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		AttackTimer = GetNode<Timer>("AttackTimer");
		SpecialActionTimer = GetNode<Timer>("SpecialActionTimer");
		AttackDelayTimer = GetNode<Timer>("AttackDelayTimer");
	}
}
