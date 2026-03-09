using Godot;
using System;

public partial class Player_JumpState : Player_PlayerState
{
    private void ClearTimers()
    {
        Player.JumpBufferTimer = 0.0f;
        Player.CoyoteTimer = 0.0f;
    }
    protected override void Enter()
    {
        ClearTimers();

        Vector2 velocity = Player.Velocity;
        velocity.Y = -Player.JumpVelocity;
        Player.Velocity = velocity;
        
        Player.TargetVisualScale = new Vector2(0.92f, 1.1f);
        AskTransit("MidAir");
    }
}