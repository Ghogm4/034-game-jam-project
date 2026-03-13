using Godot;
using System;

public partial class Player_GroundedState : Player_PlayerState
{
	protected override void Enter()
	{
		Vector2 velocity = Player.Velocity;
		ApplyLandingSquash(Player.LandingImpactSpeed);
		Player.LandingImpactSpeed = 0.0f;

		if (velocity.Y > 0.0f)
		{
			velocity.Y = 0.0f;
			Player.Velocity = velocity;
		}
	}

	private void ApplyLandingSquash(float landingSpeed)
	{
		if (landingSpeed <= 0.0f)
		{
			return;
		}

		float velocityRange = Mathf.Max(Player.LandingSquashVelocityRange, 0.001f);
		float impact = Mathf.Clamp(landingSpeed / velocityRange, 0.0f, 1.0f);
		Player.ImpactVisualScale = Vector2.One.Lerp(Player.LandingSquashScale, impact);
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
