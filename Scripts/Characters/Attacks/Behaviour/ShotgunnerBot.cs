using Enemies;
using Godot;
using System;
using static Forms.Form;

public partial class ShotgunnerBot : StateController {


	public override void OnAttackEnded() => ChangeState("ToSpecialAttack");

	public void OnSpecialStarted() => Enemy.Form.SpecialAction();

	public void SpecialAttack(float delta) {
		if (Enemy.Form.CurrentState != State.UsingSpecialAction)
			ToOutside();
	}



}
