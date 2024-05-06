using Godot;

namespace HUD;

public partial class HealthBar : PanelContainer {
	private HBoxContainer _hBoxContainer;

	[Export]
	private PackedScene _healthContainerPrefab;
	[Export]
	private Color _unactiveContainerColor;

	public override void _Ready() {
		_hBoxContainer = GetNode<HBoxContainer>("HBoxContainer");
	}

	private void LoadContainers(int currentHealth, int maxHealth) {
		foreach (Node child in _hBoxContainer.GetChildren()) child.QueueFree();

		for (int i = 1; i <= maxHealth; i++) {
			TextureRect healthContainer = _healthContainerPrefab.Instantiate<TextureRect>();
			if (currentHealth < i) healthContainer.Modulate = _unactiveContainerColor;
			_hBoxContainer.AddChild(healthContainer);
		}
	}
}
