using Godot;
using Forms;
using SceneController;
using Players;

namespace Objects;

public partial class Altar : Area2D {
	private AnimatedSprite2D _sprite;
	private Form _form;
	private AnimatedSprite2D _formSprite;

	[Export]
	private Color _colorBeforeUnlock = Colors.Black;
	[Export]
	private Color _colorAfterUnlock = Colors.White;

	public bool WasUsed { get; private set; } = false;

	public override void _Ready() {
		_form = GetNode<Form>("Form");
		WasUsed = BaseScene.ProgressionController.IsFormUnlocked(_form.FormName);

		_formSprite = _form.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_formSprite.Modulate = WasUsed ? _colorAfterUnlock : _colorBeforeUnlock;

		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_sprite.Play();
	}

	/// <summary>
	/// Unlocks form for future transformations in the progression controller and returns the unlocked Form.
	/// </summary>
	public void UnlockForm() {
		if (!BaseScene.ProgressionController.IsFormUnlocked(_form.FormName)) {
			BaseScene.ProgressionController.FormsUnlocked.Add(_form.FormName);
		}
		_sprite.Play("use");
		_formSprite.Modulate = _colorAfterUnlock;
		WasUsed = true;

		foreach (Player player in BaseScene.Players) {
			Form form = (Form) _form.Duplicate();
			form.GetNode<AnimatedSprite2D>("AnimatedSprite2D").Modulate = Colors.White;
			player.StartTranformation(form);
		}
	}
}
