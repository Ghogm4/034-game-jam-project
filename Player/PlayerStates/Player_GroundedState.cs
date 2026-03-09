using Godot;
using System;

public partial class Player_GroundedState : Player_PlayerState
{
	protected override void Enter()
	{
		Vector2 velocity = Player.Velocity;
		if (velocity.Y > 0.0f)
		{
			velocity.Y = 0.0f;
			Player.Velocity = velocity;
		}

        Player.TargetVisualScale = new Vector2(1.08f, 0.92f);
	}
	protected override void PhysicsUpdate(double delta)
	{
		if (CanJump())
		{
			DoJump();
			return;
		}

		if (!Player.IsOnFloor())
		{
			AskTransit("MidAir");
		}
	}
}