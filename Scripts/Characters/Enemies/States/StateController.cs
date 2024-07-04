using System.Linq;
using System.Collections.Generic;
using Godot;
using GodotStateCharts;
using Characters;
using Players;

namespace Enemies.States;

public partial class StateController : Node {
	private StateChart _stateChart;
	private Enemy _enemy;

	public readonly List<Character> CharactersInDetectionRange = new();
	public readonly List<Character> CharactersInAttackRange = new();

	#nullable enable
	private Character? Target { get => _enemy.Target; set => _enemy.Target = value; }

	public override void _Ready() {
		_stateChart = StateChart.Of(GetNode("StateChart"));
		_enemy = GetParent<Enemy>();
	}

	public override void _PhysicsProcess(double delta) {
		if (Target is not null && Target.IsDefeated) {
			CharactersInDetectionRange.Remove(Target);
			CharactersInAttackRange.Remove(Target);
			Target = SelectNextTarget();
		}
	}

	public Character? SelectNextTarget() {
		if (CharactersInAttackRange.FirstOrDefault() is Character targetInAttackRange) {
			if (_enemy.IsReadyToAttack) _stateChart.CallDeferred("send_event", "ToAttacking");
			return targetInAttackRange;
		}

		if (CharactersInDetectionRange.FirstOrDefault() is Character targetInDetectionRange) {
			if (_enemy.IsReadyToAttack) _stateChart.SendEvent("ToChasing");
			return targetInDetectionRange;
		}

		if (_enemy.IsReadyToAttack) _stateChart.SendEvent("ToIdle");
		return null;
	}

	public void OnAttackEnded() => _stateChart.SendEvent("ToCooldown");
	public void OnParalyzed() => _stateChart.SendEvent("ToParalyzed");
	public void OnDefeated() => _stateChart.SendEvent("ToDefeated");
	public void OnStunned() => _stateChart.SendEvent("ToStunned");

	public void OnBodyEnteredDetectionRange(Node2D body) {
		if (body is not Player player) return;
		CharactersInDetectionRange.Add(player);
		if (Target is null) {
			Target = player;
			if (_enemy.IsReadyToAttack) _stateChart.SendEvent("ToChasing");
		}
	}

	public void OnBodyExitedDetectionRange(Node2D body) {
		if (body is not Player player) return;
		CharactersInDetectionRange.Remove(player);
		if (Target == player) Target = SelectNextTarget();
	}

	public void OnBodyEnteredAttackRange(Node2D body) {
		if (body is not Player player) return;
		if (CharactersInAttackRange.Count == 0) {
			Target = player;
			if (_enemy.IsReadyToAttack) _stateChart.CallDeferred("send_event", "ToAttacking");
		}
		CharactersInAttackRange.Add(player);
	}

	public void OnBodyExitedAttackRange(Node2D body) {
		if (body is not Player player) return;
		CharactersInAttackRange.Remove(player);
		if (Target == player) Target = SelectNextTarget();
	}
}
