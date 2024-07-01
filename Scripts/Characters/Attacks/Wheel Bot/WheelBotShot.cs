using Godot;

namespace Attacks.WheelBot;

public partial class WheelBotShot : DoorOpeningAttack {
    [Export]
    private float _speed;
    private int _direction;

    public override void _Ready() {
        _direction = Sprite.FlipH ? -1 : 1;
    }

    public override void _PhysicsProcess(double delta) {
        Position += new Vector2(_speed * _direction, 0);
    }

    public void OnDurationEnded() {
		Sprite.Play("hit");
        _direction = 0;
        GetNode<Timer>("Timer").Start();
    }

    public override void OnCharacterEntered(Node2D node) {
        base.OnCharacterEntered(node);
        OnDurationEnded();
    }
}
