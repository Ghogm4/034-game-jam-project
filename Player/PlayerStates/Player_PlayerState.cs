using Godot;
using System;

public partial class Player_PlayerState : State
{
    protected Player Player => field ??= Owner as Player;

    protected float ReadMoveInput()
    {
        if (Player.IsInCutScene)
        {
            return Mathf.Clamp(Player.CutSceneMoveDir, -1.0f, 1.0f);
        }

        return Input.GetAxis("Left", "Right");
    }

    protected bool ReadJumpPressed()
    {
        if (!Player.IsInCutScene)
        {
            return Input.IsActionJustPressed("Jump");
        }

        bool cutSceneJumped = Player.CutSceneJumped;
        Player.CutSceneJumped = false;
        return cutSceneJumped;
    }

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