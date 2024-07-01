using Enemies;
using Godot;
using System;

public partial class Flower : StateController {
	public Timer ForwardTimer;
	public Timer BackwardTimer;
	int counter = 0;
	// speed of backAndForth movement
	float speedMultiplier = 0.4f;
	
	//number of times the flower will move forward and backward (1 it does forward ADN backward 1 time)
	int backAndForth = 2;
	

	public override void OnAttackEnded() => ChangeState("ToBackward");
	public void TimeoutBackward() {
		ChangeState("ToForward");
		counter++;
	}

	public void TimeoutForward() {

		if (counter > backAndForth) {
			ToOutside();
			counter = 0;
		} else ChangeState("ToBackward");
		
		counter++;
	}

	public override void _Ready() {
		base._Ready();
		ForwardTimer = WalkTimer();
		BackwardTimer = WalkTimer();

		ForwardTimer.Timeout += TimeoutForward;
		BackwardTimer.Timeout += TimeoutBackward;

	}


	private Timer WalkTimer() {
		Timer t = new Timer();
		t.OneShot = true;
		t.WaitTime = 0.4f;
		AddChild(t);
		return t;
	}

	public void Forward(float delta) {
		if (ForwardTimer.IsStopped()) {
			ForwardTimer.Start();
		}
		// TODO needs change to not fall into a hole
		Enemy.Walk(delta, 1*speedMultiplier);
	}

	public void Backward(float delta) {
		if (BackwardTimer.IsStopped()) {
			BackwardTimer.Start();
		}
		// TODO needs change to not fall into a hole
		Enemy.Walk(delta, -1* speedMultiplier);
	}



}
