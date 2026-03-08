using Godot;
using System;

public partial class Player_MidAirState : Player_PlayerState
{
    protected override void PhysicsUpdate(double delta)
    {
        if (Player.IsOnFloor())
        {
            AskTransit(IsMovingHorizontally() ? "Move" : "Idle");
        }
    }
    private bool IsMovingHorizontally()
    {
        return !Mathf.IsZeroApprox(Player.MoveInput) || Mathf.Abs(Player.Velocity.X) > Player.IdleSpeedThreshold;
    }
}
