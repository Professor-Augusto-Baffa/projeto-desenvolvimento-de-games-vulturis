
/*
using System.Linq;
using System.Collections.Generic;
using Godot;
using GodotStateCharts;
using Characters;
using Players;

namespace Enemies;

public partial class StateController : Node {
	private StateChart _stateChart;
	private Enemy _enemy;

	private readonly List<Character> _charactersInDetectionRange = new();
	private readonly List<Character> _charactersInAttackRange = new();

	#nullable enable
	private Character? Target { get => _enemy.Target; set => _enemy.Target = value; }

	public override void _Ready() {
		_stateChart = StateChart.Of(GetNode("StateChart"));
		_enemy = GetParent<Enemy>();
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
			if (_enemy.CanChangeState) _stateChart.CallDeferred("send_event", "ToAttacking");
			return targetInAttackRange;
		}

		if (_charactersInDetectionRange.FirstOrDefault() is Character targetInDetectionRange) {
			if (_enemy.CanChangeState) _stateChart.SendEvent("ToChasing");
			return targetInDetectionRange;
		}

		if (_enemy.CanChangeState) _stateChart.SendEvent("ToIdle");
		return null;
	}

	public void OnAttackEnded() => _stateChart.SendEvent("ToCooldown");
	public void OnParalyzed() => _stateChart.SendEvent("ToParalyzed");
	public void OnDefeated() => _stateChart.SendEvent("ToDefeated");
	public void OnStunned() => _stateChart.SendEvent("ToStunned");

	public void OnBodyEnteredDetectionRange(Node2D body) {
		if (body is not Player player) return;
		_charactersInDetectionRange.Add(player);
		if (Target is null) {
			Target = player;
			if (_enemy.CanChangeState) _stateChart.SendEvent("ToChasing");
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
			if (_enemy.CanChangeState) _stateChart.CallDeferred("send_event", "ToAttacking");
		}
		_charactersInAttackRange.Add(player);
	}

	public void OnBodyExitedAttackRange(Node2D body) {
		if (body is not Player player || !_charactersInAttackRange.Contains(player)) return;
		_charactersInAttackRange.Remove(player);
		if (Target == player) Target = SelectNextTarget();
	}
}
*/