using Godot;
using System;

public partial class Player_UniversalState : Player_PlayerState
{
    private bool _canPickup = true;
    private void TickJumpTimers(double delta)
    {
        float frameDelta = (float)delta;

        if (ReadJumpPressed())
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
    private void TryCollectFragment()
    {
        if (!Input.IsActionJustPressed("Interact") || Player.HeldFragment != null) return;

        Area2D pickupArea = Player.PickupArea;
        if (pickupArea == null)
        {
            GD.PushWarning("TryCollectFragment: Called by a non-player entity.");
            _canPickup = false;
            return;
        }

        foreach (Fragment fragment in Player.PickupArea.GetOverlappingAreas())
        {
            if (!fragment.Monitoring) continue;

            Player.HeldFragment = fragment;
            fragment.Collect();
            break;
        }
    }
    private void UpdateFragmentPosition()
    {
        if (Player.HeldFragment == null) return;

        float rotationBasedXOffset = Player.Visual.Rotation * Player.FragmentXAmplitude;
        float directionBasedXOffset = Player.FacingDirection * Player.FragmentDefaultHoldDistance;
        Vector2 xHoldOffset = Vector2.Right * (rotationBasedXOffset + directionBasedXOffset);
        Vector2 yHoldOffset = Vector2.Up * Mathf.Max(0, Player.Velocity.Y) * Player.FragmentYAmplitude;
        Vector2 holdOffset = xHoldOffset + yHoldOffset;
        Vector2 targetPos = Player.GlobalPosition + holdOffset;

        Vector2 fragmentPos = Player.HeldFragment.GlobalPosition;
        Player.HeldFragment.GlobalPosition = fragmentPos.Lerp(targetPos, Player.FragmentHoldLerpSpeed);
    }
    protected override void PhysicsUpdate(double delta)
    {
        bool wasOnFloor = Player.IsOnFloor();
        float preMoveVerticalSpeed = Player.Velocity.Y;

        Player.MoveInput = ReadMoveInput();
        if (!Mathf.IsZeroApprox(Player.MoveInput))
        {
            Player.FacingDirection = Player.MoveInput > 0.0f ? 1 : -1;
        }

        TickJumpTimers(delta);
        Player.MoveAndSlide();

        bool landedThisFrame = !wasOnFloor && Player.IsOnFloor();
        Player.LandingImpactSpeed = landedThisFrame ? Mathf.Max(preMoveVerticalSpeed, 0.0f) : 0.0f;

        if (!_canPickup) return;
        TryCollectFragment();
        UpdateFragmentPosition();
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
        float impactWeight = 1.0f - Mathf.Exp(-Player.ImpactVisualScaleSmoothing * (float)delta);
        Vector2 targetVisualScale = new(
            Player.BaseVisualScale.X * Player.ImpactVisualScale.X,
            Player.BaseVisualScale.Y * Player.ImpactVisualScale.Y
        );

        Player.Visual.Scale = Player.Visual.Scale.Lerp(targetVisualScale, scaleWeight);
        Player.ImpactVisualScale = Player.ImpactVisualScale.Lerp(Vector2.One, impactWeight);
    }
    protected override void FrameUpdate(double delta)
    {
        Player.VisualTime += (float)delta;
        HandleTilt(delta);
        HandleSquash(delta);
    }
}