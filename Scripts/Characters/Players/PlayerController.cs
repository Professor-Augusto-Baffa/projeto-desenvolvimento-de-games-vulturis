using Godot;

namespace Players;

public partial class PlayerController : Node {
    private Player _player;

    [ExportCategory("Input Actions")]
    [Export]
    private StringName _moveLeft;
    [Export]
    private StringName _moveRight;
    [Export]
    private StringName _jump;
    [Export]
    private StringName _dodge;
    [Export]
    private StringName _attack;
    [Export]
    private StringName _specialAction;
    [Export]
    private StringName _interaction;

    public override void _Ready() {
        _player = GetParent<Player>();
    }

    public override void _Process(double delta) {
        if (_player.CanMove) {
            _player.Direction.X = Input.GetAxis(_moveLeft, _moveRight);
        }
    }

    public override void _Input(InputEvent @event) {
        if (@event.IsActionPressed(_attack)  && _player.CanAttack) {
            _player.Attack();
        } else if (@event.IsActionPressed(_specialAction) && _player.CanUseSpecialAction) {
            _player.SpecialAction();
        } else if (@event.IsActionPressed(_dodge) && _player.CanDodge) {
            _player.StartDodge();
        } else if (@event.IsActionPressed(_interaction) && _player.CanInteract) {
            _player.LookForInteractions();

        } else if (_player.CanJump) {
            if (@event.IsActionPressed(_jump)) {
                _player.Jump();
            } else if (@event.IsActionReleased(_jump)) {
                _player.StopJumping();
            }
        }
    }
}
