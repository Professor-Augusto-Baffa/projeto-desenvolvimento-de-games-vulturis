using Godot;

namespace HUD;

public partial class AmmoBar : HBoxContainer {
	[Export]
	private PackedScene _ammoContainerPrefab;
	[Export]
	private Color _unactiveContainerColor;

	public void LoadContainers(int currentAmmo, int maxAmmo) {
		foreach (Node child in GetChildren()) child.QueueFree();

		for (int i = 1; i <= maxAmmo; i++) {
			TextureRect ammoContainer = _ammoContainerPrefab.Instantiate<TextureRect>();
			if (currentAmmo < i) ammoContainer.Modulate = _unactiveContainerColor;
			AddChild(ammoContainer);
		}
	}
}
