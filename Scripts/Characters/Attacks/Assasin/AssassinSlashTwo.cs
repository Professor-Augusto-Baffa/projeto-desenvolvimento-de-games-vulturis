namespace Attacks.Assassin;

public partial class AssassinSlashTwo : AttackBase {
	public override void _Ready() {
		GetParent().MoveChild(childNode: this, toIndex: 0); // move sprite to the back
	}
}
