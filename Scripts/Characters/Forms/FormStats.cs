using Godot;
using Attacks;
using Godot.Collections;

namespace Forms;

[GlobalClass]
public partial class FormStats : Resource {
	[Export]
	public Shape2D CollisonShape;

	[Export]
	public int MaxHealth { get; private set; }
	[Export]
	public int MaxAmmo { get; private set; }

	[Export(PropertyHint.Range, "0,100,5")]
	public int StunChance { get; private set; }

	[Export]
	public float Speed { get; private set; }

	public enum JumpHeight { NoJump, Default, HighJump }
	[Export]
	public JumpHeight Jump { get; private set; } = JumpHeight.Default;

	[Export]
	public Array<AttackStats> Attacks { get; private set; } = new();

	[ExportGroup("Special Action")]
	[Export]
	public float SpecialActionDuration { get; private set; }
	[Export]
	public int SpecialActionAmmoCost { get; private set; }
}
