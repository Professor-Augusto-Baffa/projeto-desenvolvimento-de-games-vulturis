using Godot;
using Enemies;

namespace Attacks.Flower;

public partial class FlowerSpores : AttackBase {
	public override void OnCharacterEntered(Node2D node) {
		base.OnCharacterEntered(node);
		if (node is Enemy enemy && !enemy.IsStunned && enemy.IsDefeated) {
			enemy.GetStunned();
		}
	}
}
