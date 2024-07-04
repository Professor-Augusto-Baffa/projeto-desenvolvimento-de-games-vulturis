using System;
using System.Collections.Generic;
using Godot;
using Characters;
using Players;
using Enemies;
using SceneController;
using Attacks.Assassin;
using Attacks.BlindingSpider;
using Attacks.Flower;
using Attacks.ShotgunnerBot;
using Attacks.WheelBot;

namespace Attacks;

public abstract partial class AttackBase : Area2D {
	protected AnimatedSprite2D Sprite;
	protected Character AttackUser;

	[Export]
	protected int Damage { get; private set; }

    private Vector2 Offset => new(
		x: Sprite.FlipH ? -Position.X : Position.X,
		y: Position.Y
	);

	private const string _attackPath = "res://Prefabs/Characters/Attacks/";
	private static readonly Dictionary<Type, PackedScene> _prefabs = new() {
		{ typeof(AssassinCrossSlice), GD.Load<PackedScene>(_attackPath + "Assassin/AssassinCrossSlice.tscn") },
		{ typeof(AssassinSlashOne), GD.Load<PackedScene>(_attackPath + "Assassin/AssassinSlashOne.tscn") },
		{ typeof(AssassinSlashTwo), GD.Load<PackedScene>(_attackPath + "Assassin/AssassinSlashTwo.tscn") },
		{ typeof(AssassinSweep), GD.Load<PackedScene>(_attackPath + "Assassin/AssassinSweep.tscn") },
		{ typeof(BlindingSpiderBlind), GD.Load<PackedScene>(_attackPath + "Blinding Spider/BlindingSpiderBlind.tscn") },
		{ typeof(FlowerSpores), GD.Load<PackedScene>(_attackPath + "Flower/FlowerSpores.tscn") },
		{ typeof(ShockSweeperSlam), GD.Load<PackedScene>(_attackPath + "Shock Sweeper/ShockSweeperSlam.tscn") },
		{ typeof(ShockSweeperSpinSlam), GD.Load<PackedScene>(_attackPath + "Shock Sweeper/ShockSweeperSpinSlam.tscn") },
		{ typeof(ShockSweeperSweep), GD.Load<PackedScene>(_attackPath + "Shock Sweeper/ShockSweeperSweep.tscn") },
		{ typeof(ShotgunnerBotBashOne), GD.Load<PackedScene>(_attackPath + "Shotgunner Bot/ShotgunnerBotBashOne.tscn") },
		{ typeof(ShotgunnerBotBashTwo), GD.Load<PackedScene>(_attackPath + "Shotgunner Bot/ShotgunnerBotBashTwo.tscn") },
		{ typeof(ShotgunnerBotShot), GD.Load<PackedScene>(_attackPath + "Shotgunner Bot/ShotgunnerBotShot.tscn") },
		{ typeof(WheelBotFireDash), GD.Load<PackedScene>(_attackPath + "Wheel Bot/WheelBotFireDash.tscn") },
		{ typeof(WheelBotShot), GD.Load<PackedScene>(_attackPath + "Wheel Bot/WheelBotShot.tscn") },
	};

	private const int _playerLayerNumber = 2;
	private const int _enemyLayerNumber = 3;

	public void OnAnimationFinished() => QueueFree();
	public void OnTimerTimeout() => SetCollisionMask(false, false);

	#nullable enable
	/// <summary>
	/// Factory method that instantiates attacks easily. Please specify the attack type using generics.
	/// </summary>
	/// <typeparam name="T">The specific type of attack returned, which must inherit from AttackBase.</typeparam>
	/// <param name="attackUser">The character that is using the attack, being a Player or Enemy.</param>
	/// <param name="shouldAffectEnemies">Whether the attack should affect enemies or not.</param>
	/// <param name="shouldAffectPlayers">Whether the attack should affect players or not.</param>
	/// <param name="currentScenePosition">
	/// If null, attaches the attack to the attackUser. If not, attaches it to the current scene and uses moves it to the position passed.
	/// </param>
	public static T Instantiate<T>(Character attackUser, bool shouldAffectEnemies, bool shouldAffectPlayers, Vector2? currentScenePosition = null) where T : AttackBase {
		T attack = _prefabs[typeof(T)].Instantiate<T>();

		attack.AttackUser = attackUser;
        attack.SetCollisionMask(shouldAffectEnemies, shouldAffectPlayers);

		Node parent = BaseScene.ActiveScene;
		if (currentScenePosition == null) {
			parent = attackUser;
			currentScenePosition = Vector2.Zero;
		}

		attack.SetSpritePosition(attackUser.Sprite.FlipH, currentScenePosition.Value);
        parent.AddChild(attack);

		return attack;
	}

	protected void SetSpritePosition(bool flipH, Vector2 position) {
        Sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        Sprite.FlipH = flipH;
		Sprite.Play();

        Position = position + Offset;
	}

	protected void SetCollisionMask(bool shouldAffectEnemies, bool shouldAffectPlayers) {
		SetCollisionMaskValue(_playerLayerNumber, shouldAffectPlayers);
		SetCollisionMaskValue(_enemyLayerNumber, shouldAffectEnemies);
	}

	public virtual void OnCharacterEntered(Node2D node) {
		if (node == AttackUser || (node is Character character && character.IsInvincible)) return;

		if (node is Player player) {
			HitPlayer(player);
		} else if (node is Enemy enemy) {
			HitEnemy(enemy);
		}
	}

	protected virtual void HitPlayer(Player player) {
		player.TakeDamage(Damage);
		SetCollisionMask(false, false);
	}

	protected virtual void HitEnemy(Enemy enemy) {
		enemy.TakeDamage(Damage, Sprite.FlipH);
		SetCollisionMask(false, false);
	}
}
