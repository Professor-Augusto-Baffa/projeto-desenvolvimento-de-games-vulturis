using Godot;

namespace UI;

public partial class ScrollableRichTextLabel : RichTextLabel {
	[Export]
	private int _scrollSpeed;

	private VScrollBar _scrollBar;

	public override void _Ready() {
		_scrollBar = GetVScrollBar();
	}

	public override void _Process(double delta) {
		if (Input.IsActionPressed("ui_up") && _scrollBar.Value - _scrollSpeed > 0) {
			_scrollBar.Value -= _scrollSpeed;
		} else if (Input.IsActionPressed("ui_down") && _scrollBar.Value + _scrollSpeed < _scrollBar.MaxValue) {
			_scrollBar.Value += _scrollSpeed;
		}
	}
}
