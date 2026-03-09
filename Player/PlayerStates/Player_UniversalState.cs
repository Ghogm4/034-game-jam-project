using Godot;
using System;

public partial class Player_UniversalState : Player_PlayerState
{
    private void TickJumpTimers(double delta)
    {
        float frameDelta = (float)delta;

        if (Input.IsActionJustPressed("Jump"))
        {
            Player.JumpBufferTimer = Player.JumpBufferTime;
        }
        else
        {
            Player.JumpBufferTimer = Mathf.Max(Player.JumpBufferTimer - frameDelta, 0.0f);
        }

        if (Player.IsOnFloor())
        {
            Player.CoyoteTimer = Player.CoyoteTime;
        }
        else
        {
            Player.CoyoteTimer = Mathf.Max(Player.CoyoteTimer - frameDelta, 0.0f);
        }
    }
    protected override void PhysicsUpdate(double delta)
    {
        bool wasOnFloor = Player.IsOnFloor();
        float preMoveVerticalSpeed = Player.Velocity.Y;

        Player.MoveInput = Input.GetAxis("Left", "Right");
        if (!Mathf.IsZeroApprox(Player.MoveInput))
        {
            Player.FacingDirection = Player.MoveInput > 0.0f ? 1 : -1;
        }

        TickJumpTimers(delta);
        Player.MoveAndSlide();

        bool landedThisFrame = !wasOnFloor && Player.IsOnFloor();
        Player.LandingImpactSpeed = landedThisFrame ? Mathf.Max(preMoveVerticalSpeed, 0.0f) : 0.0f;
    }
    private void HandleTilt(double delta)
    {
        float moveRatio = Mathf.IsZeroApprox(Player.MoveSpeed) ? 0.0f : Player.Velocity.X / Player.MoveSpeed;
        float targetRotation = Mathf.DegToRad(Player.TiltAngleDegrees) * Mathf.Clamp(moveRatio, -1.0f, 1.0f);
        float rotationWeight = 1.0f - Mathf.Exp(-Player.TiltSmoothing * (float)delta);
        Player.Visual.Rotation = Mathf.Lerp(Player.Visual.Rotation, targetRotation, rotationWeight);
    }
    private void HandleSquash(double delta)
    {
        float scaleWeight = 1.0f - Mathf.Exp(-Player.VisualScaleSmoothing * (float)delta);
        Player.Visual.Scale = Player.Visual.Scale.Lerp(Player.TargetVisualScale, scaleWeight);
        Player.TargetVisualScale = Player.TargetVisualScale.Lerp(Vector2.One, scaleWeight);
    }
    protected override void FrameUpdate(double delta)
    {
        Player.VisualTime += (float)delta;
        HandleTilt(delta);
        HandleSquash(delta);
    }
}