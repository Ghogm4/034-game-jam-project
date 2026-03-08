using Godot;
using System;

public partial class Player_UniversalState : Player_PlayerState
{
    protected override void PhysicsUpdate(double delta)
    {
        Player.MoveInput = Input.GetAxis("Left", "Right");
        if (!Mathf.IsZeroApprox(Player.MoveInput))
        {
            Player.FacingDirection = Player.MoveInput > 0.0f ? 1 : -1;
        }

        Player.MoveAndSlide();

        if (Player.IsOnFloor() && Input.IsActionJustPressed("Jump"))
        {
            AskTransit("Jump");
        }
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
        HandleTilt(delta);
        HandleSquash(delta);
    }
}