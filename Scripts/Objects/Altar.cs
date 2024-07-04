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

	[Signal]
	public delegate void FormUnlockedEventHandler();

	public bool IsBeingUsed = false;
	private bool _wasUsed = false;

	public override void _Ready() {
		_form = GetNode<Form>("Form");
		_wasUsed = BaseScene.ProgressionController.IsFormUnlocked(_form.FormName);

		_formSprite = _form.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_formSprite.Modulate = _wasUsed ? _colorAfterUnlock : _colorBeforeUnlock;

		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_sprite.Play();
	}

	public async void Activate() {
		if (!_wasUsed) {
			UnlockForm();
		} else {
			TransformPlayers();
		}

		IsBeingUsed = true;
		_sprite.Play("use");
		await ToSignal(_sprite, AnimatedSprite2D.SignalName.AnimationFinished);
		IsBeingUsed = false;
	}

	/// <summary>
	/// Unlocks form for future transformations in the progression controller and returns the unlocked Form.
	/// </summary>
	private void UnlockForm() {
		if (!BaseScene.ProgressionController.IsFormUnlocked(_form.FormName)) {
			BaseScene.ProgressionController.FormsUnlocked.Add(_form.FormName);
		}
		_wasUsed = true;

		GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D").Play();
		_formSprite.Modulate = _colorAfterUnlock;
		TransformPlayers();
		EmitSignal(SignalName.FormUnlocked);
	}

	private void TransformPlayers() {
		foreach (Player player in BaseScene.Players) {
			Form form = (Form) _form.Duplicate();
			form.GetNode<AnimatedSprite2D>("AnimatedSprite2D").Modulate = Colors.White;
			player.StartTranformation(form);
		}
	}
}
