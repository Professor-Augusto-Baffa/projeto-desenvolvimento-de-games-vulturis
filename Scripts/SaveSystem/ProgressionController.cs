using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace SaveSystem;

public partial class ProgressionController : Node {
	public void SetValuesFrom(SaveFileModel save) {
		FormsUnlocked = save.FormsUnlocked;
		HealthUpgradesCollected = new HashSet<string>(save.HealthUpgradesCollected);
		AmmoUpgradesCollected = new HashSet<string>(save.AmmoUpgradesCollected);
		TutorialsCompleted = new HashSet<string>(save.TutorialsCompleted);
	}

	/// <summary>
	/// Array containing all forms that are unlocked and can be used in transformations.
	/// Forms are represented by their class names as strings.
	/// </summary>
	[Export]
	public Array<string> FormsUnlocked = new();
	public bool IsFormUnlocked(string form) => FormsUnlocked.Contains(form);

	/// <summary>
	/// HashSet containing the name of all scenes where health upgrades were collected.
	/// Use the HealthUpgradesAmount property to get the max health boost.
	/// </summary>
	public HashSet<string> HealthUpgradesCollected = new();
	public bool CollectedHealthUpgradeIn(string scene) => HealthUpgradesCollected.Contains(scene);
	public int HealthUpgradesAmount => HealthUpgradesCollected.Count;

	public HashSet<string> AmmoUpgradesCollected = new();
	public bool CollectedAmmoUpgradeIn(string scene) => AmmoUpgradesCollected.Contains(scene);
	public int AmmoUpgradesAmount => AmmoUpgradesCollected.Count;

	public HashSet<string> TutorialsCompleted = new();
}
