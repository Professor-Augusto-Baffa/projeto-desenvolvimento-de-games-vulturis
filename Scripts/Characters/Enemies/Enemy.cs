using System.Collections.Generic;
using System.Linq;
using Godot;
using Characters;
using Forms;
using Players;

namespace Enemies;

public partial class Enemy : Character {
	public new Form Form => base.Form;
	public Timer _paralyzeTimer;
	private Timer _fadeOutTimer;
	public Timer _coolDownTimer;

	public override bool IsInvincible => _currentState == State.Stunned || _currentState == State.Paralyzed || _currentState == State.Defeated;

	public override bool IsDefeated => _currentState == State.Defeated;
	public bool IsStunned => _currentState == State.Stunned;

	public bool IsBeeingUsedInTransformation = false; // Used to avoid multiple players transforming into a single enemy

	private const int _deathAnimationDuration = 1;

	public enum State { Idle, Stunned, Paralyzed, Defeated, Attack, Chase, CoolDown }
	[Export]
	public State _currentState = State.Idle;
	private FMS _stateMachine;

	[ExportGroup("Hit flash")]
	[Export]
	private Shader _hitFlashShader;
	[Export]
	private float _flashDuration;

	// FIXME move this to state machine
	// checks if the player is in the attack range of the enemy
	public bool InAttackRange = false;
	// checks if the player is in the detection range of the enemy
	public bool InDetectionRange = false;
	private readonly List<Character> _charactersInDetectionRange = new();
	private readonly List<Character> _charactersInAttackRange = new();

	#nullable enable
	private Character? _target;

	public override void _Ready() {
		base._Ready();
		_stateMachine = GetNode<FMS>("FMS");
		_paralyzeTimer = GetNode<Timer>("Timers/ParalyzeTimer");
		_fadeOutTimer = GetNode<Timer>("Timers/FadeOutTimer");
		_coolDownTimer = GetNode<Timer>("Timers/CoolDownTimer");
		Health = MaxHealth;
	}

	public override void _PhysicsProcess(double delta) {
		_currentState = _stateMachine.NextState();

		if (_target is not null && _target.IsDefeated) {
			if (_charactersInDetectionRange.Contains(_target)) {
				_charactersInDetectionRange.Remove(_target);
				InDetectionRange = false;
			}
			if (_charactersInAttackRange.Contains(_target)) {
				_charactersInAttackRange.Remove(_target);
				InAttackRange = false;
			}
			_target = LookForNextTarget();
		}
		
		// if form attack timer is attacking and not in the attack state stop the attack
		if (Form.CurrentState == Form.State.Attacking && _currentState != State.Attack) {
			Form.OnAttackEnded();
		}

		switch (_currentState) {
			case State.Defeated:
				Fade(delta);
				break;
			case State.Attack:
				if (Form.CanAttack) Form.Attack();
				break;
			case State.Idle:
				Idle(delta);
				UpdateMovementAnimations();
				break;
			case State.Chase:
				Chase(delta);
				UpdateMovementAnimations();
				break;
			case State.CoolDown:
				if(_coolDownTimer.IsStopped()) CoolDown();
				break;
			default:
				break;
		}
	}
	
	/// <summary>
	/// Function to handle the end of cooldown of the enemy attack
	/// </summary>
	public void OnCoolDownTimeout() {
		_currentState = State.Idle;
	}
	
	/// <summary>
	/// Function to handle the end of the attack of the enemy
	/// </summary>
	public void CoolDown() {
		_currentState = State.CoolDown;
		_coolDownTimer.Start();
	}

	protected override void UpdateMovementAnimations() {
		Sprite.Play(this.Velocity != Vector2.Zero ? "walk" : "idle");

		if (_target != null) {
			Sprite.FlipH = this.Position.X > _target.Position.X;
		}
	}

	/// <summary>
	/// Function to handle Idle State in a "Physical" way
	/// </summary>
	/// <param name="delta"></param>
	public void Idle(double delta) {
		// I don't know exactly whaty pathfining gives, I'm going to belive it is a vector
		// TODO: deals with needed jumps... 
		Walk(delta, 0);
	}

	/// <summary>
	/// Function to handle Chase State in a "Physical" way
	/// </summary>
	/// <param name="delta"></param>
	public void Chase(double delta) {
		// pathfind to the nearest player
		float direction = this.Position.X < _target!.Position.X ? 1 : -1;
		Walk(delta, direction);
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

	public void TakeDamage(int damage, bool attackFlipH, float? frameFreezeDuration = null) {
		Health -= damage;
		if (Health <= 0) {
			if (IsStunnedByRandomChance(attackedFromBehind: attackFlipH == Sprite.FlipH)) {
				GetStunned();
			}
			else {
				Die();
			}
		}

		Flash();
		FrameFreeze(frameFreezeDuration);
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
		_currentState = State.Stunned;
	}

	public override async void StartParalyze(float paralyzeDuration) {
		this.Velocity = Vector2.Zero;
		_currentState = State.Paralyzed;
		await ToSignal(
			source: GetTree().CreateTimer(paralyzeDuration),
			signal: SceneTreeTimer.SignalName.Timeout
		);
		_currentState = State.Idle;
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
		_currentState = State.Defeated;
		Sprite.Play("death");
		await ToSignal(
			source: GetTree().CreateTimer(_deathAnimationDuration),
			signal: SceneTreeTimer.SignalName.Timeout
		);
		_fadeOutTimer.Start();
	}

	private void Fade(double delta) {
		Sprite.Modulate = Sprite.Modulate.Lerp(to: Colors.Transparent, weight: (float)(delta / _fadeOutTimer.WaitTime));
	}

	public void OnFadeOut() => QueueFree();

	private Character? LookForNextTarget() {
		if (_charactersInAttackRange.FirstOrDefault() is Character targetInAttackRange) {
			InAttackRange = true;
			return targetInAttackRange;
		}

		if (_charactersInDetectionRange.FirstOrDefault() is Character targetInDetectionRange) {
			InDetectionRange = true;
			return targetInDetectionRange;
		}

		
		InDetectionRange = false;
		InAttackRange = false;
		
		return null;
	}

	public void OnBodyEnteredDetectionRange(Node2D body) {
		if (body is not Player player) return;
		_charactersInDetectionRange.Add(player);
		if (_target == null) {
			_target = player;
			InDetectionRange = true;
		}
	}

	public void OnBodyExitedDetectionRange(Node2D body) {
		if (body is not Player player || !_charactersInDetectionRange.Contains(player)) return;
		_charactersInDetectionRange.Remove(player);
		InDetectionRange = false;
		if (_target == player) _target = LookForNextTarget();
	}

	public void OnBodyEnteredAttackRange(Node2D body) {
		if (body is not Player player) return;
		if (_charactersInAttackRange.Count == 0) {
			_target = player;
			InAttackRange = true;
		}
		_charactersInAttackRange.Add(player);
	}

	public void OnBodyExitedAttackRange(Node2D body) {
		if (body is not Player player || !_charactersInAttackRange.Contains(player)) return;
		_charactersInAttackRange.Remove(player);
		InAttackRange = false;
		if (_target == player) _target = LookForNextTarget();
	}
}
