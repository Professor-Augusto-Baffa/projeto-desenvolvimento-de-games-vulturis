using Enemies;
using Godot;
using System;

public partial class BlindingSpider : StateController {
	public Timer AttackDelayTimer;
	public Timer WalkingTimer;
	int CurrentPlayerDirection = 0;

	public void StartDelayedAttack() => Enemy.StartAttack();
	public void Back(float delta){
		if(WalkingTimer.IsStopped()) ToOutside();
		else Enemy.WalkWithAnimation(delta,CurrentPlayerDirection);
	}
	public override void OnAttackEnded(){
		CurrentPlayerDirection = PlayerDirection();
		WalkingTimer.Start();
		ChangeState("ToBack");
	}
	
	public int PlayerDirection(){
		if(Enemy.Target is null) return 0;
		return Enemy.PlayerDirection();
	}

	public override void Attack(float delta) {
		if(AttackDelayTimer.IsStopped()) {
			Enemy.Attack(delta);
		}
	}
	public override void StartAttack() {
		if(AttackDelayTimer.IsStopped()) AttackDelayTimer.Start();
	}

	public override void _Ready() {
		base._Ready();
		AttackDelayTimer = _Timer();
		WalkingTimer = _Timer();
	
		AttackDelayTimer.Timeout += StartDelayedAttack;
		WalkingTimer.Timeout += ToOutside;
	}
	
	public void InterruptAttack() {
		AttackDelayTimer.Stop();
	}
	
	private Timer _Timer() {
		Timer t = new Timer();
		t.OneShot = true;
		t.WaitTime = 0.5f;
		AddChild(t);
		return t;
	}
	
	

}
