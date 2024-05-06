using Godot;
using Objects;

namespace Attacks.ShotgunnerBot;

public partial class ShotgunnerBotShot : AttackBase {
    public override void OnCharacterEntered(Node2D node) {
        base.OnCharacterEntered(node);
        if (node is Door door && !door.IsOpen) door.Open();
    }
}
