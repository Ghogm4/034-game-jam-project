using Godot;
using System;

public partial class Player_PlayerState : State
{
    protected Player Player => field ??= Owner as Player;
    protected bool CanJump()
    {
        return Player.JumpBufferTimer > 0.0f && Player.CoyoteTimer > 0.0f;
    }

    protected void DoJump()
    {
        Player.JumpBufferTimer = 0.0f;
        Player.CoyoteTimer = 0.0f;
        AskTransit("Jump");
    }
}