using Godot;
using Characters;

namespace Attacks;

public abstract partial class ParalyzingAttack : AttackBase {
    [Export]
    private float _paralizeDuration = 1;

    public override void OnCharacterEntered(Node2D node) {
        base.OnCharacterEntered(node);
        if (node is Character character && !character.IsDefeated) {
            character.StartParalyze(paralyzeDuration: _paralizeDuration, shouldApplyEffects: true);
        }
    }
}
