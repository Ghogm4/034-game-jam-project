using Godot;
using System;

public partial class Player_IdleState : Player_PlayerState
{
	protected override void FrameUpdate(double delta)
	{
		Player.TargetVisualScale = GetIdleVisualScale();
	}

	protected override void PhysicsUpdate(double delta)
	{
		if (ShouldTransitToMove())
		{
			AskTransit("Move");
		}
	}
    private bool ShouldTransitToMove()
    {
        return !Mathf.IsZeroApprox(Player.MoveInput) || Mathf.Abs(Player.Velocity.X) > Player.IdleSpeedThreshold;
    }

	private Vector2 GetIdleVisualScale()
	{
		float pulse = Mathf.Sin(Player.VisualTime * Player.IdleSquashSpeed);
		return Vector2.One.Lerp(Player.IdleSquashScale, 0.5f + pulse * 0.5f);
	}
}