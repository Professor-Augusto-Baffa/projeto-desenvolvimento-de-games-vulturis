using Godot;
using Forms;

namespace Characters;

public abstract partial class Character : CharacterBody2D {
    public Form Form { get; protected set; }
	protected FormStats FormStats => Form.FormStats;

	public AnimatedSprite2D Sprite => Form.Sprite;
	protected AudioStreamPlayer2D SoundEffectsPlayer;
	protected string[] AnimationNames;

	protected int Health;
	protected int MaxHealth => FormStats.MaxHealth;
    public abstract bool IsInvincible { get; }
	public abstract bool IsDefeated { get; }

	[Export]
	private float _frameFreezeDuration;

	[Export]
	protected Color ParalyzeColor = new("ffff7f");

    [ExportGroup("Jump")]
	[Export]
	protected float JumpSpeed;
	[Export]
	private float _defaultJumpDuration = .25f;
	[Export]
	private float _highJumpDuration = .35f;
	protected float JumpDuration {
		get => FormStats.Jump switch {
			FormStats.JumpHeight.Default => _defaultJumpDuration,
			FormStats.JumpHeight.HighJump => _highJumpDuration,
			FormStats.JumpHeight.NoJump => 0,
			_ => 0,
		};
	}

	[ExportGroup("Sound Effects")]
	[Export] protected AudioStream JumpSound;
	[Export] protected AudioStream LandingSound;
	[Export] protected AudioStream ParalyzeSound;

	protected static readonly float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public override void _Ready() {
        Form = GetNode<Form>("Form");
        AnimationNames = Sprite.SpriteFrames.GetAnimationNames();
		SoundEffectsPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
    }

	protected abstract void UpdateMovementAnimations();
    public abstract void StartParalyze(float paralyzeDuration, bool shouldApplyEffects = false);

	protected async void FrameFreeze(double? duration = null) {
		duration ??= _frameFreezeDuration;

		Engine.TimeScale = .05;
		await ToSignal(
			source: GetTree().CreateTimer(timeSec: duration.Value, ignoreTimeScale: true),
			signal: SceneTreeTimer.SignalName.Timeout
		);
		Engine.TimeScale = 1;
	}
}
