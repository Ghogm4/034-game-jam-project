using Godot;
using System;

public partial class Player_IdleState : Player_PlayerState
{
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
}