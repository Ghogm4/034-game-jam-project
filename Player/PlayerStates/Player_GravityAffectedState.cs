using Godot;
using System;

public partial class Player_GravityAffectedState : Player_PlayerState
{
	protected override void PhysicsUpdate(double delta)
	{
		Vector2 velocity = Player.Velocity;
		velocity += Player.GetGravity() * (float)delta * Player.GravityScale;
		velocity.Y = Mathf.Min(velocity.Y, Player.MaxFallSpeed);
		Player.Velocity = velocity;
	}
}