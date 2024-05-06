using Godot;

namespace Attacks;

[GlobalClass]
public partial class AttackStats : Resource {
    /// <summary>
    /// Attack total duration in seconds. The duration characters is considered to be attacking, usually the animation duration.
    /// </summary>
    [Export]
    public float Duration { get; private set; }
    /// <summary>
    /// Duration in seconds before instantiating the attack prefab with the damage collider and animations.
    /// Keep it at 0 to instantiate immediately.
    /// </summary>
    [Export]
    public float StartingDelay { get; private set; }
}
