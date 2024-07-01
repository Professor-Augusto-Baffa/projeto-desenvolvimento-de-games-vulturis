using Enemies;
using Godot;
using Players;
using System;
using static Forms.Form;

public partial class WheelBot : StateController {

	bool PlayerNear = false;
	bool HasUsedSpecial = false;

	public override void OnAttackEnded() {
		CharInRange();
		if (PlayerNear && !HasUsedSpecial) {
			HasUsedSpecial = true;
			ChangeState("ToSpecialAttack");
		}
		else ToOutside();
	}

	public void CharInRange() {
		if (_charactersInAttackRange.Count > 0) PlayerNear = true;
		else PlayerNear = false;
	}

	public void SpecialAttack(float delta) {

		if (Enemy.Form.CurrentState != State.UsingSpecialAction)
			ToOutside();
	}

	public void OnSpecialStarted() => Enemy.Form.SpecialAction();

}
