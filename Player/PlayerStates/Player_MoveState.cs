using Godot;
using System;

public partial class Player_MoveState : Player_PlayerState
{
	protected override void FrameUpdate(double delta)
	{
		Player.TargetVisualScale = GetMoveVisualScale();
	}

	protected override void PhysicsUpdate(double delta)
	{
		if (ShouldTransitToIdle())
		{
			AskTransit("Idle");
		}
	}
    private bool ShouldTransitToIdle()
    {
        return Mathf.IsZeroApprox(Player.MoveInput) && Mathf.Abs(Player.Velocity.X) <= Player.IdleSpeedThreshold;
    }

	private Vector2 GetMoveVisualScale()
	{
		float normalizedSpeed = Mathf.IsZeroApprox(Player.MoveSpeed)
			? 0.0f
			: Mathf.Clamp(Mathf.Abs(Player.Velocity.X) / Player.MoveSpeed, 0.0f, 1.0f);
		float stride = Mathf.Sin(Player.VisualTime * Player.MoveSquashSpeed);
		float strideWeight = normalizedSpeed * (0.5f + stride * 0.5f);
		return new Vector2(1.0f + Player.MoveSquashStrength * strideWeight, 1.0f - Player.MoveSquashStrength * strideWeight);
	}
}