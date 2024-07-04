using Enemies;

namespace Attacks.Flower;

public partial class FlowerSpores : AttackBase {
    protected override void HitEnemy(Enemy enemy) {
		enemy.TakeDamage(Damage, Sprite.FlipH, shouldForceStun: true);
		SetCollisionMask(false, false);
    }
}
