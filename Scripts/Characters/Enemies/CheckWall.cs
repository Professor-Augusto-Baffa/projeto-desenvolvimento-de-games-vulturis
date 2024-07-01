using Godot;
using System;
using Enemies;
using System.Linq.Expressions;

public partial class CheckWall : RayCast2D {

	Enemy _enemy;
	CollisionShape2D _collisionShape2D;


	public override void _Ready() {
		_enemy = GetParent<Enemy>();
		_collisionShape2D = _enemy.GetNode<CollisionShape2D>("CollisionShape2D");

		//Pega o collision shape do inimigo
		Vector2 ColisionPosition = _collisionShape2D.Shape.GetRect().End;
		
		// Se for o Vetor Pra Esquerda
		if(TargetPosition.X < 0){
			ColisionPosition.X = -ColisionPosition.X;
		}
		
		Position = ColisionPosition;
		
	}

	public override void _PhysicsProcess(double delta) {
		if (IsColliding() && GetCollider() is TileMap) {
			if (TargetPosition.X < 0) {
				_enemy.lastObstacle = true;
			}
			else {
				_enemy.lastObstacle = false;
			}
		}
	}


}
