using Godot;
using System;

public partial class Player_CanMoveState : Player_PlayerState
{
	protected override void PhysicsUpdate(double delta)
	{
		float targetSpeed = Player.MoveInput * Player.MoveSpeed;
		float acceleration = GetAcceleration();

		Vector2 velocity = Player.Velocity;
		velocity.X = Mathf.MoveToward(velocity.X, targetSpeed, acceleration * (float)delta);
		Player.Velocity = velocity;
	}
    private float GetAcceleration()
    {
        return Mathf.IsZeroApprox(Player.MoveInput)
			? (Player.IsOnFloor() ? Player.GroundDeceleration : Player.AirDeceleration)
			: (Player.IsOnFloor() ? Player.GroundAcceleration : Player.AirAcceleration);
    }
}