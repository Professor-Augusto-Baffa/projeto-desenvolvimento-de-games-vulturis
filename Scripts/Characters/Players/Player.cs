using System.Linq;
using Godot;
using Godot.Collections;
using Characters;
using Forms;
using SaveSystem;
using SceneController;
using Objects;
using Enemies;

namespace Players;

public partial class Player : Character {
	private CollisionShape2D _shape;
	private Area2D _interactionRange;
	private ProgressionController _progressionController;

	private Timer _ammoRecoveryTimer;
	private Timer _jumpTimer;
	private Timer _transformationTimer;
	private Timer _dodgeTimer;
	private Timer _playerHealingTimer;
	private Timer _invincibilityAfterDamageTimer;
	private Timer _paralyzeTimer;

	private new int MaxHealth => FormStats.MaxHealth + _progressionController.HealthUpgradesAmount;
	public override bool IsInvincible {
		get => _currentState == State.Dodging || _currentState == State.Defeated || _invincibilityAfterDamageTimer.TimeLeft > 0;
	}
	public override bool IsDefeated => _currentState == State.Defeated;

	private int _ammo;
	private int MaxAmmo => (FormStats.MaxAmmo == 0) ? 0 : FormStats.MaxAmmo + _progressionController.AmmoUpgradesAmount;
	private int Ammo {
		get => _ammo;
		set {
			_ammo = Mathf.Clamp(value, 0, MaxAmmo);
			EmitSignal(SignalName.AmmoChanged, _ammo, MaxAmmo);
		}
	}

	public string CurrentFormName => Form.FormName;

	[Export]
	private float _dodgeSpeed;

	[Export]
	private float _jumpCoyoteTimeDuration;

	[Export]
	private bool _friendlyFire = false;

	[Export]
	private float _damageParalizeDuration;

	[ExportGroup("Camera shake after damage")]
	[Export]
	private float _shakeIntensity;
	[Export]
	private float _shakeDuration;

	[Signal]
	public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);
	[Signal]
	public delegate void AmmoChangedEventHandler(int currentAmmo, int maxAmmo);

	/// <summary>
	/// The direction the player is facing in the X axis.
	/// -1: Left, 1: Right, 0: Stops horizontal movement
	/// </summary>
	public float HorizontalDirection { private get; set; } = 0;
	private bool _isJumping = false;

	#nullable enable
	private Form? _nextForm;
	private Player? _playerBeingHealed;

	private enum State { Default, Dodging, Paralyzed, Transforming, HealingPlayer, Defeated }
	private State _currentState = State.Default;

	private bool CanStartAnotherAction => _currentState == State.Default && Form.CurrentState == Form.State.Idle;

	public bool CanMove => _currentState == State.Default && Form.CurrentState != Form.State.UsingSpecialAction;
	public bool CanJump { get; private set; } = false;
	public bool CanAttack => _currentState == State.Default && !_isJumping && Form.CanAttack;
	public bool CanUseSpecialAction {
		get => _currentState == State.Default && Form.CanUseSpecialAction && FormStats.SpecialActionAmmoCost <= Ammo;
	}
	public bool CanDodge => CanStartAnotherAction && IsOnFloor();
	public bool CanInteract => CanStartAnotherAction && IsOnFloor();

	public override void _Ready() {
        base._Ready();

		_progressionController = GetNode<ProgressionController>("../ProgressionController");;
		_shape = GetNode<CollisionShape2D>("CollisionShape2D");
		_interactionRange = GetNode<Area2D>("InteractionRange");

		_ammoRecoveryTimer = GetNode<Timer>("Timers/AmmoRecoveryTimer");
		_jumpTimer = GetNode<Timer>("Timers/JumpTimer");
		_dodgeTimer = GetNode<Timer>("Timers/DodgeTimer");
		_transformationTimer = GetNode<Timer>("Timers/TransformationTimer");
		_playerHealingTimer = GetNode<Timer>("Timers/PlayerHealingTimer");
		_invincibilityAfterDamageTimer = GetNode<Timer>("Timers/InvincibilityAfterDamageTimer");
		_paralyzeTimer = GetNode<Timer>("Timers/ParalyzeTimer");

		UpdateForm();
	}

    public override void _PhysicsProcess(double delta) {
		if (Form.CurrentState != Form.State.Attacking && HorizontalDirection != 0) {
			Sprite.FlipH = HorizontalDirection < 0;
		}

		if (CanMove) {
			if (CanStartAnotherAction && IsOnFloor() && FormStats.Jump != FormStats.JumpHeight.NoJump) {
				CanJump = true;
			}
			if (CanJump && !_isJumping && !IsOnFloor()) {
				StartCoyoteTime();
			}

			Move(delta);
			if (Form.CurrentState == Form.State.Idle) UpdateMovementAnimations();

		} else if (_currentState == State.Dodging) {
			Dodge(delta);
		} else if (_currentState == State.Defeated && !IsOnFloor()) {
			Fall(delta);
		}
    }

	protected override void UpdateMovementAnimations() {
		StringName animation = "idle";

		if (_isJumping) {
			animation = AnimationNames.Contains("jump") ? "jump" : "idle";
		} else if (!IsOnFloor()) {
			animation = AnimationNames.Contains("fall") ? "fall" : "idle";
		} else if (HorizontalDirection != 0) {
			animation = "walk";
		}

		Sprite.Play(animation);
	}

	public void TakeDamage(int damage, bool shouldParalize = true, float? frameFreezeDuration = null) {
		Health -= damage;
		EmitSignal(SignalName.HealthChanged, Health, MaxHealth);
		if (_currentState == State.Transforming) StopTransforming();
		if (_currentState == State.HealingPlayer) StopHealingPlayer();

		if (Health <= 0) {
			StopMoving();
			StringName animation = AnimationNames.Contains("stun") ? "stun" : "death";
			Sprite.Play(animation);
			// TODO add player death signal
			_currentState = State.Defeated;
		} else {
			Sprite.Stop();
			Sprite.Play("hit");
			_invincibilityAfterDamageTimer.Start();
			if (shouldParalize) StartParalyze(_damageParalizeDuration);
		}

		BaseScene.Camera.StartShake(_shakeIntensity, _shakeDuration);
		FrameFreeze(frameFreezeDuration);
	}

	public void Heal(int? healthHealed = null) {
		healthHealed ??= MaxHealth;
		Health += healthHealed.Value;
		Health = Mathf.Clamp(Health, 0, MaxHealth);
		EmitSignal(SignalName.HealthChanged, Health, MaxHealth);
	}

	public void RestoreAmmo() {
		Ammo++;
		if (Ammo >= MaxAmmo) _ammoRecoveryTimer.Stop();
	}

    private void Move(double delta) {
		float xVelocity = HorizontalDirection * FormStats.Speed;
		if (Form.CurrentState == Form.State.Attacking) xVelocity /= 3;

		float yVelocity;
		if (_isJumping) {
			yVelocity = -JumpSpeed;
		} else {
			yVelocity = this.Velocity.Y + (float) (Gravity * delta);
		}

		this.Velocity = new Vector2(xVelocity, yVelocity);
		MoveAndSlide();
	}

	private void StopMoving() {
		HorizontalDirection = 0;
		this.Velocity = Vector2.Zero;
	}

	private void Fall(double delta) {
		this.Velocity = new Vector2(0, this.Velocity.Y + (float) (Gravity * delta));
		MoveAndSlide();
	}

	public void Jump() {
		_isJumping = true;
		if (JumpDuration <= 0) return; // necessary in order to avoid timer errors
        _jumpTimer.WaitTime = JumpDuration;
		_jumpTimer.Start();
	}

	public void StopJumping() {
		_isJumping = false;
		CanJump = false;
		_jumpTimer.Stop();
	}

	private async void StartCoyoteTime() {
		await ToSignal(
			source: GetTree().CreateTimer(_jumpCoyoteTimeDuration),
			signal: SceneTreeTimer.SignalName.Timeout
		);
		CanJump = false;
	}

	public void StartDodge() {
		_dodgeTimer.Start();

		StringName animation = AnimationNames.Contains("dodge") ? "dodge" : "idle";
		Sprite.Play(animation);

		_currentState = State.Dodging;
	}

	private void Dodge(double delta) {
		float xVelocity = _dodgeSpeed;
		if (!Sprite.FlipH) xVelocity *= -1;
		float yVelocity = this.Velocity.Y + (float) (Gravity * delta);

		this.Velocity = new Vector2(xVelocity, yVelocity);
		MoveAndSlide();

		if (!IsOnFloor() || IsOnWall()) {
			StopDodge();
		}
	}

	public void StopDodge() {
		_dodgeTimer.Stop();
		_currentState = State.Default;
	}

	public void Attack() {
		CanJump = false;
		Form.Attack();
	}

	public void SpecialAction() {
		Ammo -= FormStats.SpecialActionAmmoCost;
		_ammoRecoveryTimer.Start();
		HorizontalDirection = 0;
		Form.SpecialAction();
	}

	public override void StartParalyze(float paralyzeDuration) {
		StopMoving();
		_currentState = State.Paralyzed;
		_paralyzeTimer.WaitTime = paralyzeDuration;
		_paralyzeTimer.Start();
	}

	public void StopParalyze() {
		_paralyzeTimer.Stop();
		_currentState = State.Default;
	}

	public void LookForInteractions() {
		if (LookForDefeatedPlayers() is Player player) {
			StartHealing(player);

		} else if (LookForAltar() is Altar altar) {
			altar.UnlockForm();

		} else if (LookForStunnedEnemy() is Enemy enemy) {
			enemy.IsBeeingUsedInTransformation = true;
			StartTranformation(enemy.Form);
		}
	}

	private void StartHealing(Player player) {
		_playerBeingHealed = player;
		StopMoving();
		Sprite.Play("idle");
		_playerHealingTimer.Start();
		_currentState = State.HealingPlayer;
	}

	public void HealPlayer() {
		if (_playerBeingHealed == null) return;
		_playerBeingHealed.Heal(MaxHealth);
		_playerBeingHealed._currentState = State.Default;
		StopHealingPlayer();
	}

	private void StopHealingPlayer() {
		_playerHealingTimer.Stop();
		_playerBeingHealed = null;
		_currentState = State.Default;
	}

	public void StartTranformation(Form form) {
		_nextForm = form;
		StopMoving();
		Sprite.Play("death");
		_transformationTimer.Start();
		_currentState = State.Transforming;
	}

	public void UpdateForm() {
		// Remove previous Form if there was one
		if (_nextForm != null) {
			Form.QueueFree();

			if (_nextForm.GetParent() is Enemy enemy) {
				this.Position = enemy.Position;
				enemy.QueueFree();
				enemy.RemoveChild(_nextForm);
			}

			AddChild(_nextForm);
			Form = _nextForm;
			_nextForm = null;
		}

		Heal();
		Ammo = FormStats.MaxAmmo;
		AnimationNames = Sprite.SpriteFrames.GetAnimationNames();
		_shape.Shape = FormStats.CollisonShape;
		Form.UpdateAttackCollisions(shouldAffectEnemies: true, shouldAffectPlayers: _friendlyFire);
		StopTransforming();
	}

	private void StopTransforming() {
		_transformationTimer.Stop();
		_currentState = State.Default;

		if (_nextForm == null || _nextForm.GetParent() is not Enemy enemy) return;
		enemy.IsBeeingUsedInTransformation = false;
		_nextForm = null;
	}

	private Player? LookForDefeatedPlayers() {
		Array<Node2D> bodies = _interactionRange.GetOverlappingBodies();
		Player? node = (Player?) bodies.FirstOrDefault(
			predicate: (node) => node is Player player && player.IsDefeated
		);
		return node;
	}

	private Enemy? LookForStunnedEnemy() {
		Array<Node2D> bodies = _interactionRange.GetOverlappingBodies();

		// Filter enemies among all bodies
		Array<Enemy> stunnedEnemies = new();
		foreach (Node2D body in bodies) {
            if (body is not Enemy enemy) continue;
			if (!enemy.IsStunned || enemy.IsBeeingUsedInTransformation) continue; // filter stunned enemies available for transformation
			if (!_progressionController.IsFormUnlocked(enemy.Form.FormName)) continue; // filter if form was unlocked
			stunnedEnemies.Add(enemy);
        }
		if (stunnedEnemies.Count == 0) return null;

		// Find closest enemy
		Enemy closestEnemy = stunnedEnemies[0];
		float closestDistance = closestEnemy.Position.DistanceSquaredTo(this.Position);
		foreach (Enemy enemy in stunnedEnemies) {
			float distance = enemy.Position.DistanceSquaredTo(this.Position);
			if (distance < closestDistance) {
				closestEnemy = enemy;
				closestDistance = distance;
			}
		}

		return closestEnemy;
	}

	private Altar? LookForAltar() {
		Array<Area2D> areas = _interactionRange.GetOverlappingAreas();
		Altar? area = (Altar?) areas.FirstOrDefault(
			predicate: (area) => area is Altar altar && !altar.WasUsed
		);
		return area;
	}
}
