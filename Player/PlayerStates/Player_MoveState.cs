using Godot;
using System;

public partial class Player_MoveState : Player_PlayerState
{
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
}