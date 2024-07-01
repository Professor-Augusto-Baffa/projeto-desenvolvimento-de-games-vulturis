using Enemies;
using Godot;
using System;

public partial class CheckHole : RayCast2D {

	Enemy _enemy;

	public override void _Ready() {
		_enemy = GetParent<Enemy>();
	}

	public override void _PhysicsProcess(double delta) {


		if (IsColliding() && GetCollider() is TileMap) {
			if (Position.X < 0) {
				_enemy.IsHoleL = false;
			} else {
				_enemy.IsHoleR = false;
			}
		}
		else {
			if (Position.X < 0) {
				_enemy.IsHoleL = true;
				_enemy.lastObstacle = true;
			} else {
				_enemy.IsHoleR = true;
				_enemy.lastObstacle = false;
			}
		}
	}

}
