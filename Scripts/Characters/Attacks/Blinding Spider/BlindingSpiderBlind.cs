using Godot;
using Characters;

namespace Attacks.BlindingSpider;

public partial class BlindingSpiderBlind : AttackBase {
	[Export]
	private float _paralizeDuration;

	public override void OnCharacterEntered(Node2D node) {
		base.OnCharacterEntered(node);
		if (node is Character character && !character.IsDefeated) {
			character.StartParalyze(_paralizeDuration);
		}
	}
}
