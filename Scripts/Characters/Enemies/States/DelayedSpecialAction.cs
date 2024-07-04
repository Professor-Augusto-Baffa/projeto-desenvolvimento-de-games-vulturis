using Godot;

namespace Enemies.States;

/// <summary>
/// In this State, enemies will use their special action after a delay defined in DelayTimer.
/// This action will only happen if there are characters in attack range.
/// Otherwise, the state finishes before the duration specified in the cooldown Timer.
/// </summary>
/// <remarks>
/// So, in this CooldownState, the default cooldown timer indicates the maximum duration of the state.
/// But the state could also take only the duration of the delay.
/// </remarks>
public partial class DelayedSpecialAction : CooldownState {
	private Timer _delayTimer;
	private StateController _stateController;

	public override void _Ready() {
		base._Ready();
		_delayTimer = GetNode<Timer>("DelayTimer");
		_stateController = Enemy.GetNode<StateController>("StateController");
	}

    public override void OnStart() {
        base.OnStart();
		_delayTimer.Start();
    }

    public override void OnEnd() {
        base.OnEnd();
		_delayTimer.Stop();
    }

	public void OnDelayEnded() {
		if (_stateController.CharactersInAttackRange.Count > 0 && Enemy.Form.CanUseSpecialAction) {
			Enemy.StartSpecialAction();
		} else {
			OnEnd();
		}
	}
}
