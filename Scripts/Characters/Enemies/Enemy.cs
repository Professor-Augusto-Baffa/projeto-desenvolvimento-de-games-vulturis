using System.Linq;
using Godot;
using Characters;
using Forms;
using Menus.Settings;

namespace Enemies;

public partial class Enemy : Character {
	public Timer _paralyzeTimer;
	private Timer _fadeOutTimer;
	private Timer _coolDownTimer;

	public int PlayerDirection () => this.Position.X < Target!.Position.X ? 1 : -1;

	public override bool IsInvincible => IsDefeated || IsStunned || _isParalyzed;
	public override bool IsDefeated => Health <= 0;

	public bool IsStunned { get; private set; }
	private bool _isParalyzed; // TODO move to StateChart
	public bool CanChangeState => !_isParalyzed && _coolDownTimer.IsStopped();

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

	// check if there is a Hole in the left or right
	public bool IsHoleL = false;
	public bool IsHoleR = false;
	
	// if true, the last hole or wall was at left, if false, the last hole or wall was at right
	public bool lastObstacle = true;
	

	public override void _Ready() {
		base._Ready();
		_paralyzeTimer = GetNode<Timer>("Timers/ParalyzeTimer");
		_fadeOutTimer = GetNode<Timer>("Timers/FadeOutTimer");
		_coolDownTimer = GetNode<Timer>("Timers/CoolDownTimer");
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
		Walk(delta, 0);
		UpdateMovementAnimations();
	}

	/// <summary>
	/// Function to handle Chase State in a "Physical" way
	/// </summary>
	/// <param name="delta"></param>
	public void Chase(double delta) {
		// pathfind to the nearest player
		float direction = PlayerDirection();
		if (IsHoleR && direction == 1) {
			direction *= 0;
		} else if(IsHoleL && direction == -1) {
			direction *= 0;
		}
		Walk(delta, direction);
		UpdateMovementAnimations();
	}
	

	/// <summary>
	/// Function handle for walking
	/// </summary>
	/// <param name="delta"> Right or left</param>
	/// <param name="direction"></param>
	public void Walk(double delta, float direction) {
		this.Velocity = new Vector2(direction * FormStats.Speed, this.Velocity.Y + (float)(Gravity * delta));
		MoveAndSlide();
	}
	
	public void WalkWithAnimation(double delta, float direction) {
		// If there is a hole in that dire
		if((direction > 0 && IsHoleR) || (direction < 0 && IsHoleL)) 
			direction = 0;
		
		Walk(delta, direction);
		UpdateMovementAnimations();
	}

	protected override void UpdateMovementAnimations() {
		Sprite.Play(this.Velocity != Vector2.Zero ? "walk" : "idle");

		if (Target != null) {
			Sprite.FlipH = this.Position.X > Target.Position.X;
		}
		if(this.Velocity.X < 0)
			Sprite.FlipH = true;
		else if(this.Velocity.X > 0)
			Sprite.FlipH = false;
	}

	public void StartAttack() {
		if (Form.CanAttack) Form.Attack();
	}

	public void Attack(double delta) {
		if (Form.CurrentState == Form.State.Idle) {
			EmitSignal(SignalName.AttackEnded);
		}
		else {
			if (Form.CanAttack) Form.Attack();
		}
	}

	public async void StartCooldown() {
		_coolDownTimer.Start();
		Sprite.Play("idle");
		await ToSignal(source: _coolDownTimer, signal: Timer.SignalName.Timeout);
		EmitSignal(SignalName.NewTargetNeeded);
	}

	public override async void StartParalyze(float paralyzeDuration, bool shouldApplyEffects = false) {
		this.Velocity = Vector2.Zero;
		_isParalyzed = true;
		EmitSignal(SignalName.Paralyzed);
		if (shouldApplyEffects) {
			Sprite.Modulate = ParalyzeColor;
			SoundEffectsPlayer.Stream = ParalyzeSound;
			SoundEffectsPlayer.Play();
		}
		await ToSignal(
			source: GetTree().CreateTimer(paralyzeDuration),
			signal: SceneTreeTimer.SignalName.Timeout
		);

		Sprite.Modulate = Colors.White;
		_isParalyzed = false;
		EmitSignal(SignalName.NewTargetNeeded);
	}

	public async void TakeDamage(int damage, bool attackFlipH, float? frameFreezeDuration = null) {
		Health -= damage;

		SoundEffectsPlayer.Stream = Form.DamageSound;
		SoundEffectsPlayer.Play();

		if (IsDefeated) {
			if (IsStunnedByRandomChance(attackedFromBehind: attackFlipH == Sprite.FlipH)) {
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
