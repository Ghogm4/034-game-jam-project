using Godot;
using System;

public partial class Player_MidAirState : Player_PlayerState
{
	protected override void FrameUpdate(double delta)
	{
		Player.BaseVisualScale = GetMidAirVisualScale();
	}

	protected override void PhysicsUpdate(double delta)
	{
		if (CanJump())
		{
			DoJump();
			return;
		}
		if (Player.IsOnFloor())
		{
			AskTransit(IsMovingHorizontally() ? "Move" : "Idle");

			if (!Player.IsInCutScene) AudioManager.Instance.PlaySFX("Land Ground");
		}
	}
	private bool IsMovingHorizontally()
	{
		return !Mathf.IsZeroApprox(Player.MoveInput) || Mathf.Abs(Player.Velocity.X) > Player.IdleSpeedThreshold;
	}

	private Vector2 GetMidAirVisualScale()
	{
		float normalizedVerticalSpeed = Mathf.Clamp(Player.Velocity.Y / Player.MidAirVisualVelocityRange, -1.0f, 1.0f);
		if (normalizedVerticalSpeed >= 0.0f)
		{
			return Vector2.One.Lerp(Player.MidAirFallScale, normalizedVerticalSpeed);
		}

		return Vector2.One.Lerp(Player.MidAirRiseScale, -normalizedVerticalSpeed);
	}
}
