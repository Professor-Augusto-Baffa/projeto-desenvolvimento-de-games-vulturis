using System;
using Enemies;
using Forms;
using Godot;

/// <summary>
/// Finite State Machine specific for enemies
/// </summary>
public partial class FMS : Node2D {

	private Form form;
	private Enemy enemy;

	// let the state machine change
	public Func<Enemy.State> NextState;

	public override void _Ready() {
		form = GetNode<Form>("../Form");
		enemy = GetNode<Enemy>("..");

		if (form is Flower) {
			NextState = Flower;
		}
		else if (form is Assassin) {
			NextState = Assassin;
		}
		else if (form is ShotgunnerBot) {
			NextState = Shotgunner;
		}
		else if (form is WheelBot) {
			NextState = WheelBot;
		}
		else if (form is BlindingSpider) {
			NextState = BlindingSpider;
		}
	}

	// state machine for the Flower enemy
	public Enemy.State Flower() {

		// TODO: end it
		if (enemy.IsDefeated) {
			return Enemy.State.Defeated;
		}
		if (enemy.IsStunned) { // missing in range of player
			return Enemy.State.Stunned;
		}
		
		if (enemy._paralyzeTimer.IsStopped() == false){
			return Enemy.State.Paralyzed;
		}

		if (enemy._currentState == Enemy.State.CoolDown || (
			enemy._currentState == Enemy.State.Attack &&
			form.CurrentState == Form.State.Idle)) {
				
			return Enemy.State.CoolDown;
		}

		if (enemy.InDetectionRange) {
			if (enemy.InAttackRange)
				return Enemy.State.Attack;

			return Enemy.State.Chase;
		}


		return Enemy.State.Idle;
	}
	//state machine for the Assassin enemy
	public Enemy.State Assassin() {
		var state = Flower();
		return state;
	}

	//state machine for the Shotgunner enemy
	public Enemy.State Shotgunner() {
		// TODO: Start it
		return Flower();
	}
	//state machine for the WheelBot enemy
	public Enemy.State WheelBot() {
		var state = Flower();
		return state;
	}

	//state machine for the BlindingSpider enemy
	public Enemy.State BlindingSpider() {
		// TODO: Start it
		return Flower();
	}

}