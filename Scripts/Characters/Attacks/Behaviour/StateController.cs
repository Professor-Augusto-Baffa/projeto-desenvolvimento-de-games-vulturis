using System.Linq;
using System.Collections.Generic;
using Godot;
using GodotStateCharts;
using Characters;
using Players;

namespace Enemies;

public partial class StateController : Node {
	private StateChart _stateChart;
	public Enemy Enemy;

	//TODO change back to private
	private readonly List<Character> _charactersInDetectionRange = new();
	public readonly List<Character> _charactersInAttackRange = new();

#nullable enable
	private Character? Target { get => Enemy.Target; set => Enemy.Target = value; }

	public override void _Ready() {
		_stateChart = StateChart.Of(GetNode("StateChart"));
		Enemy = GetParent<Enemy>();
	}

	public override void _PhysicsProcess(double delta) {
		if (Target is not null && Target.IsDefeated) {
			if (_charactersInDetectionRange.Contains(Target)) _charactersInDetectionRange.Remove(Target);
			if (_charactersInAttackRange.Contains(Target)) _charactersInAttackRange.Remove(Target);
			Target = SelectNextTarget();
		}
	}

	public Character? SelectNextTarget() {
		if (_charactersInAttackRange.FirstOrDefault() is Character targetInAttackRange) {
			if (Enemy.CanChangeState) _stateChart.CallDeferred("send_event", "ToAttacking");
			return targetInAttackRange;
		}

		if (_charactersInDetectionRange.FirstOrDefault() is Character targetInDetectionRange) {
			if (Enemy.CanChangeState) _stateChart.SendEvent("ToChasing");
			return targetInDetectionRange;
		}

		if (Enemy.CanChangeState) _stateChart.SendEvent("ToIdle");
		return null;
	}

	public void OnBodyEnteredDetectionRange(Node2D body) {
		if (body is not Player player) return;
		_charactersInDetectionRange.Add(player);
		if (Target is null) {
			Target = player;
			if (Enemy.CanChangeState) _stateChart.SendEvent("ToChasing");
		}
	}

	public void OnBodyExitedDetectionRange(Node2D body) {
		if (body is not Player player || !_charactersInDetectionRange.Contains(player)) return;
		_charactersInDetectionRange.Remove(player);
		if (Target == player) Target = SelectNextTarget();
	}

	public void OnBodyEnteredAttackRange(Node2D body) {
		if (body is not Player player) return;
		if (_charactersInAttackRange.Count == 0) {
			Target = player;
			if (Enemy.CanChangeState) _stateChart.CallDeferred("send_event", "ToAttacking");
		}
		_charactersInAttackRange.Add(player);
	}

	public void OnBodyExitedAttackRange(Node2D body) {
		if (body is not Player player || !_charactersInAttackRange.Contains(player)) return;
		_charactersInAttackRange.Remove(player);
		if (Target == player) Target = SelectNextTarget();
	}

	public virtual void Attack(float delta) => Enemy.Attack(delta);
	public virtual void StartAttack() => Enemy.StartAttack(); 


	// Check if there are any enemies in the detection range
	public bool HasEnemiesInDetectionRange() => _charactersInDetectionRange.Count > 0;
	
	public bool HasEnemiesInAttackRange() => _charactersInAttackRange.Count > 0;


	// Wrapper to Simplify State Changes calls
	public void ChangeState(string transition) => _stateChart.SendEvent(transition);


	// Funtions called when Leaving a State
	public virtual void OnAttackEnded() => _stateChart.SendEvent("ToCooldown");

	public virtual void OnParalyzed() => _stateChart.SendEvent("ToParalyzed");
	public virtual void OnDefeated() => _stateChart.SendEvent("ToDefeated");
	public virtual void OnStunned() => _stateChart.SendEvent("ToStunned");


	/// <summary>
	/// Helper Function To Change the State After Attack Behaviour
	/// </summary> <summary>
	/// 
	/// </summary>
	public void ToOutside() {
		if (Enemy.Target is not null)
			ChangeState("ToCooldown");
		else if (HasEnemiesInDetectionRange())
			ChangeState("FromBehaviourToChasing");
		else
			ChangeState("FromBehaviourToIdle");
	}

}
