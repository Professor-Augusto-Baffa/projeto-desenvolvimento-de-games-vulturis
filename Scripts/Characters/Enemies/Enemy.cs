using System.Linq;
using Godot;
using Characters;
using Forms;
using Menus.Settings;

namespace Enemies;

public partial class Enemy : Character {
	private Timer _paralyzeTimer;
	private Timer _fadeOutTimer;
	private Timer _coolDownTimer;

	public override bool IsInvincible => IsDefeated || IsStunned || _isParalyzed;
	public override bool IsDefeated => Health <= 0;

	public bool IsStunned { get; private set; }
	private bool _isParalyzed;
	public bool IsReadyToAttack => !_isParalyzed && _coolDownTimer.IsStopped();

	public bool IsBeeingUsedInTransformation = false; // Used to avoid multiple players transforming into a single enemy

	[ExportGroup("Hit flash")]
	[Export]
	private Shader _hitFlashShader;
	[Export]
	private float _flashDuration;

	[ExportGroup("Sound Effects")]
	[Export] private AudioStream StunnedSound;

	// These signals notify to the StateController the enemy should change its state
	[Signal] public delegate void NewTargetNeededEventHandler();
	[Signal] public delegate void AttackEndedEventHandler();
	[Signal] public delegate void DefeatedEventHandler();
	[Signal] public delegate void StunnedEventHandler();
	[Signal] public delegate void ParalyzedEventHandler();

	#nullable enable
	public Character? Target;

	public Vector2 TargetDirection {
		get => (this.Position.X > Target!.Position.X) ? Vector2.Left : Vector2.Right;
	}

	// check if there is a Hole in the left or right
	public bool IsHoleL = false;
	public bool IsHoleR = false;
	
	// if true, the last hole or wall was at left, if false, the last hole or wall was at right
	public bool lastObstacle = true;
	

	public override void _Ready() {
		base._Ready();
		_paralyzeTimer = GetNode<Timer>("Timers/ParalyzeTimer");
		_fadeOutTimer = GetNode<Timer>("Timers/FadeOutTimer");
		_coolDownTimer = GetNode<Timer>("CooldownBehaviour/Timer");
		Health = MaxHealth;
	}

	public override void _PhysicsProcess(double delta) {
		if (Target is not null && Target.IsDefeated) {
			EmitSignal(SignalName.NewTargetNeeded);
		}
	}

	/// <summary>
	/// Function to handle Idle State in a "Physical" way
	/// </summary>
	/// <param name="delta"></param>
	public void Idle(double delta) {
		Walk(delta, direction: Vector2.Zero);
		UpdateMovementAnimations();
	}

	/// <summary>
	/// Function to handle Chase State in a "Physical" way
	/// </summary>
	/// <param name="delta"></param>
	public void Chase(double delta) {
		Walk(delta, TargetDirection);
		UpdateMovementAnimations();
	}

	public void Walk(double delta, Vector2 direction, float speedMultiplier = 1) {
		if((direction == Vector2.Right && IsHoleR) || (direction == Vector2.Left && IsHoleL)) {
			direction = Vector2.Zero;
		}

		this.Velocity = new Vector2(
			x: direction.X * FormStats.Speed * speedMultiplier,
			y: this.Velocity.Y + (float) (Gravity * delta)
		);
		MoveAndSlide();
		UpdateMovementAnimations();
	}

	protected override void UpdateMovementAnimations() {
		string animation = this.Velocity != Vector2.Zero ? "walk" : "idle";
		Sprite.Play(animation);

		if (Target is null) {
			if (this.Velocity != Vector2.Zero) {
				Sprite.FlipH = this.Velocity.X < 0;
			}
		} else {
			TurnToTarget();
		} 
	}

	public void TurnToTarget() {
		if (Target is null) return;
		Sprite.FlipH = TargetDirection == Vector2.Left;
	}

	public void StartAttack() {
		TurnToTarget();
		if (Form.CanAttack) Form.Attack();
	}

	public void Attack(double delta) {
		if (Form.CurrentState == Form.State.Idle) {
			EmitSignal(SignalName.AttackEnded);
		} else {
			if (Form.CanAttack) Form.Attack();
		}
	}

	public void StartSpecialAction() {
		TurnToTarget();
		if (Form.CanUseSpecialAction) Form.SpecialAction();
	}

	public override async void StartParalyze(float paralyzeDuration, bool shouldApplyEffects = false) {
		this.Velocity = Vector2.Zero;
		Sprite.Stop();
		Form.StopAttack();
		if (shouldApplyEffects) {
			Sprite.Modulate = ParalyzeColor;
			SoundEffectsPlayer.Stream = ParalyzeSound;
			SoundEffectsPlayer.Play();
		}

		_isParalyzed = true;
		EmitSignal(SignalName.Paralyzed);
		await ToSignal(
			source: GetTree().CreateTimer(paralyzeDuration),
			signal: SceneTreeTimer.SignalName.Timeout
		);

		Sprite.Modulate = Colors.White;
		_isParalyzed = false;
		if (!IsDefeated) EmitSignal(SignalName.NewTargetNeeded);
	}

	public async void TakeDamage(int damage, bool attackFlipH, float? frameFreezeDuration = null, bool shouldForceStun = false) {
		Health -= damage;

		SoundEffectsPlayer.Stream = Form.DamageSound;
		SoundEffectsPlayer.Play();

		if (IsDefeated) {
			if (IsStunnedByRandomChance(attackedFromBehind: attackFlipH == Sprite.FlipH) || shouldForceStun) {
				GetStunned();
			} else {
				Die();
			}

			await ToSignal(source: SoundEffectsPlayer, signal: AudioStreamPlayer2D.SignalName.Finished);
			SoundEffectsPlayer.Stream = Form.DeathSound;
			SoundEffectsPlayer.Play();
		}

		Flash();
		if (Settings.HitStopEnabled) FrameFreeze(frameFreezeDuration);
	}

	private bool IsStunnedByRandomChance(bool attackedFromBehind) {
		int stunChance = FormStats.StunChance;
		if (attackedFromBehind) stunChance *= 2;
		// TODO add modifiers from upgrades
		return GD.RandRange(from: 1, to: 100) <= stunChance;
	}

	public void GetStunned() {
		StringName animation = AnimationNames.Contains("stun") ? "stun" : "death";
		Sprite.Play(animation);
		IsStunned = true;
		EmitSignal(SignalName.Stunned);

		SoundEffectsPlayer.Stream = StunnedSound;
		SoundEffectsPlayer.Play();
	}

	private async void Flash() {
		ShaderMaterial material = (ShaderMaterial)Form.Sprite.Material;
		material.Shader = _hitFlashShader;
		await ToSignal(
			source: GetTree().CreateTimer(_flashDuration),
			signal: SceneTreeTimer.SignalName.Timeout
		);
		material.Shader = null;
	}

	private async void Die() {
		EmitSignal(SignalName.Defeated);
		Sprite.Play("death");
		await ToSignal(source: Sprite, signal: AnimatedSprite2D.SignalName.AnimationFinished);
		_fadeOutTimer.Start();
	}

	private void Fade(double delta) {
		if (!_fadeOutTimer.IsStopped()) {
			Sprite.Modulate = Sprite.Modulate.Lerp(to: Colors.Transparent, weight: (float)(delta / _fadeOutTimer.WaitTime));
		}
	}

	public void OnFadeOut() => QueueFree();
}
