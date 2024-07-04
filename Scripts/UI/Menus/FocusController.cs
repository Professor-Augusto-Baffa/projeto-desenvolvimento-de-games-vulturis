using Godot;
using Godot.Collections;

namespace Menus;

public partial class FocusController : Control {
	[Export]
	protected Control _firstFocusedNode;

	protected bool _activatedFocus = false;

	[Export]
	private Array<StringName> _actionsThatActivateFocus = new() {
		"ui_up", "ui_down", "ui_right", "ui_left",
	};

	public override void _Input(InputEvent @event) {
		if (_activatedFocus) return;

		foreach (StringName action in _actionsThatActivateFocus) {
			if (@event.IsActionPressed(action)) {
				GrabFocus();
				break;
			}
		}
	}

    public new void GrabFocus() {
        _activatedFocus = true;
        _firstFocusedNode.GrabFocus();
    }
}
