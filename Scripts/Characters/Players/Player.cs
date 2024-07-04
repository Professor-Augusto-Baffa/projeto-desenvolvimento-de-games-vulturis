using System.Linq;
using Godot;
using Godot.Collections;
using Characters;
using Forms;
using SaveSystem;
using SceneController;
using Objects;
using Enemies;
using UI;
using Menus.MainMenu;
using Menus.Settings;

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

	[Export]
	private float _speedDuringAttackMultiplier;

	[Export]
	private float _dodgeSpeed;

	[Export]
	private float _jumpCoyoteTimeDuration;

	[Export]
	private float _damageParalizeDuration;

	[Export]
	private Color _colorDuringHealing;

	[ExportGroup("Camera shake after damage")]
	[Export]
	private float _shakeIntensity;
	[Export]
	private float _shakeDuration;

	[ExportGroup("Sound Effects")]
	[Export] private AudioStream _dodgeSound;
	[Export] private AudioStream _transformationSound;
	[Export] private AudioStream _healSound;

	[Signal]
	public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);
	[Signal]
	public delegate void AmmoChangedEventHandler(int currentAmmo, int maxAmmo);

	public Vector2 Direction = Vector2.Zero;
	private bool _isJumping = false;

	#nullable enable
	public Form? NextForm { private get; set; }
	private Player? _playerBeingHealed;

	private enum State { Default, Dodging, Paralyzed, Transforming, HealingPlayer, Defeated }
	private State _currentState = State.Default;

	public bool IsAttacking => Form.CurrentState == Form.State.Attacking;

	private bool CanStartAnotherAction => _currentState == State.Default && Form.CurrentState == Form.State.Idle;

	public bool CanMove => _currentState == State.Default && Form.CurrentState != Form.State.UsingSpecialAction;
	public bool CanJump { get; private set; } = false;
	public bool CanAttack => _currentState == State.Default && !_isJumping && Form.CanAttack;
	public bool CanUseSpecialAction {
		get => _currentState == State.Default && Form.CanUseSpecialAction && FormStats.SpecialActionAmmoCost <= Ammo;
	}
	public bool CanDodge => _currentState == State.Default && Form.CurrentState != Form.State.UsingSpecialAction && IsOnFloor() ;
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
		// Update player's direction
		if (!IsAttacking && Direction != Vector2.Zero) {
			Sprite.FlipH = Direction == Vector2.Left;
		}

		if (CanMove) {
			// Landing after jump
			if (!CanJump && CanStartAnotherAction && IsOnFloor() && FormStats.Jump != FormStats.JumpHeight.NoJump) {
				SoundEffectsPlayer.Stream = LandingSound;
				SoundEffectsPlayer.Play();
				CanJump = true;
			}

			// Enable coyote time
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
		} else if (Direction != Vector2.Zero) {
			animation = "walk";
		}

		Sprite.Play(animation);
	}

	/// <summary> Heals the player by a fixed amount. If no healthHealed argument is passed, the player health will be restored to full. /// </summary>
	public void Heal(int? healthHealed = null) {
		healthHealed ??= MaxHealth;
		Health += healthHealed.Value;
		Health = Mathf.Clamp(Health, 0, MaxHealth);
		EmitSignal(SignalName.HealthChanged, Health, MaxHealth);
	}

	/// <summary> Adds a fixed amount of ammo to the player. If no ammo argument is passed, the player will restore all ammo. </summary>
	public void AddAmmo(int? amount = null) {
		Ammo += amount ?? MaxAmmo;
	}

	public void RestoreAmmo() {
		Ammo++;
		if (Ammo >= MaxAmmo) _ammoRecoveryTimer.Stop();
	}

	public void TakeDamage(int damage, bool shouldParalize = true, float? frameFreezeDuration = null) {
		Health -= damage;
		EmitSignal(SignalName.HealthChanged, Health, MaxHealth);
		if (_currentState == State.Transforming) StopTransforming();
		if (_currentState == State.HealingPlayer) StopHealingPlayer();

		Form.StopAttack();

		if (Health <= 0) {
			OnDefeated();
		} else {
			Sprite.Stop();
			Sprite.Play("hit");
			if (Form.DamageSound != null) {
				SoundEffectsPlayer.Stream = Form.DamageSound;
				SoundEffectsPlayer.Play();
			}

			_invincibilityAfterDamageTimer.Start();
			if (shouldParalize) StartParalyze(_damageParalizeDuration);
		}

		BaseScene.Camera.StartShake(_shakeIntensity, _shakeDuration);
		if (Settings.HitStopEnabled)  FrameFreeze(frameFreezeDuration);
	}

	private async void OnDefeated() {
		_currentState = State.Defeated;
		StopMoving();

		StringName animation = AnimationNames.Contains("stun") ? "stun" : "death";
		Sprite.Play(animation);
		if (Form.DeathSound != null) {
			SoundEffectsPlayer.Stream = Form.DeathSound;
			SoundEffectsPlayer.Play();
		}

		if (BaseScene.Players.All((player) => player.IsDefeated)) {
			Sprite.Play("death");
			await ToSignal(source: Sprite, signal: AnimatedSprite2D.SignalName.AnimationFinished);
			await SceneTransition.FadeIn();

			MainMenu.StartGame(rootNode: GetParent());
		}
	}

    private void Move(double delta) {
		float xVelocity = Direction.X * FormStats.Speed;
		if (IsAttacking) xVelocity *= _speedDuringAttackMultiplier;

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
		Direction = Vector2.Zero;
		this.Velocity = Vector2.Zero;
	}

	public void Jump() {
		_isJumping = true;

		if (JumpDuration <= 0) return; // necessary in order to avoid timer errors
        _jumpTimer.WaitTime = JumpDuration;
		_jumpTimer.Start();

		SoundEffectsPlayer.Stream = JumpSound;
		SoundEffectsPlayer.Play();
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
		Form.StopAttack();

		StringName animation = AnimationNames.Contains("dodge") ? "dodge" : "idle";
		Sprite.Play(animation);

		SoundEffectsPlayer.Stream = _dodgeSound;
		SoundEffectsPlayer.Play();

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
		Direction = Vector2.Zero;
		Form.SpecialAction();
	}

	public override void StartParalyze(float paralyzeDuration, bool shouldApplyEffects = false) {
		StopMoving();
		if (shouldApplyEffects) {
			Sprite.Modulate = ParalyzeColor;
			SoundEffectsPlayer.Stream = ParalyzeSound;
			SoundEffectsPlayer.Play();
		}
		_currentState = State.Paralyzed;
		_paralyzeTimer.WaitTime = paralyzeDuration;
		_paralyzeTimer.Start();
	}

	public void StopParalyze() {
		_paralyzeTimer.Stop();
		Sprite.Modulate = Colors.White;
		_currentState = State.Default;
	}

	private void StartHealing(Player player) {
		_playerBeingHealed = player;
		StopMoving();
		Sprite.Play("idle");
		_playerBeingHealed.Sprite.Modulate = _colorDuringHealing;
		_playerHealingTimer.Start();
		_currentState = State.HealingPlayer;
	}

	public void HealPlayer() {
		if (_playerBeingHealed == null) return;
		_playerBeingHealed.Heal(MaxHealth);

		_playerBeingHealed.SoundEffectsPlayer.Stream = _healSound;
		_playerBeingHealed.SoundEffectsPlayer.Play();

		_playerBeingHealed._currentState = State.Default;
		StopHealingPlayer();
	}

	private void StopHealingPlayer() {
		if (_playerBeingHealed == null) return; 
		_playerHealingTimer.Stop();
		_playerBeingHealed.Sprite.Modulate = Colors.White;
		_playerBeingHealed = null;
		_currentState = State.Default;
	}

	public void StartTranformation(Form form) {
		NextForm = form;
		StopMoving();
		Sprite.Play("death");

		SoundEffectsPlayer.Stream = _transformationSound;
		SoundEffectsPlayer.Play();

		_transformationTimer.Start();
		_currentState = State.Transforming;
	}

	public void UpdateForm() {
		// Remove previous Form if there was one
		if (NextForm != null) {
			Form.QueueFree();

			if (NextForm.GetParent() is Enemy enemy) {
				this.Position = enemy.Position;
				enemy.QueueFree();
				enemy.RemoveChild(NextForm);
			}

			AddChild(NextForm);
			base.Form = NextForm;
			NextForm = null;
		}

		Heal();
		Ammo = MaxAmmo;
		AnimationNames = Sprite.SpriteFrames.GetAnimationNames();
		_shape.Shape = FormStats.CollisonShape;
		Form.UpdateAttackCollisions(
			shouldAffectEnemies: true,
			shouldAffectPlayers: Settings.FriendlyFireEnabled
		);
		StopTransforming();
	}

	private void StopTransforming() {
		_transformationTimer.Stop();
		_currentState = State.Default;

		if (NextForm == null || NextForm.GetParent() is not Enemy enemy) return;
		enemy.IsBeeingUsedInTransformation = false;
		NextForm = null;
	}

	public void LookForInteractions() {
		if (LookForDefeatedPlayers() is Player player) {
			StartHealing(player);

		} else if (LookForAltar() is Altar altar) {
			altar.Activate();

		} else if (LookForStunnedEnemy() is Enemy enemy) {
			enemy.IsBeeingUsedInTransformation = true;
			StartTranformation(enemy.Form);

		} else if (LookForPanel() is Objects.Panel panel) {
			panel.Save();
		}
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
			predicate: (area) => area is Altar altar && !altar.IsBeingUsed
		);
		return area;
	}

	private Objects.Panel? LookForPanel() {
		Array<Area2D> areas = _interactionRange.GetOverlappingAreas();
		Objects.Panel? area = (Objects.Panel?) areas.FirstOrDefault(
			predicate: (area) => area is Objects.Panel panel && !panel.IsBeingUsed
		);
		return area;
	}
}
