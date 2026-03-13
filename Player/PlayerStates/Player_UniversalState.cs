using Godot;
using System;

public partial class Player_UniversalState : Player_PlayerState
{
	private bool _canPickup = true;
	private Fragment _highlightedFragment = null;

	private Fragment FindNearestPickupableFragment()
	{
		CollisionShape2D pickupAreaCollision = Player.PickupArea?.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		if (pickupAreaCollision?.Shape is not CircleShape2D pickupCircle)
			return null;

		float pickupRadius = pickupCircle.Radius * Mathf.Max(
			Mathf.Abs(Player.PickupArea.GlobalScale.X),
			Mathf.Abs(Player.PickupArea.GlobalScale.Y)
		);

		Fragment nearestFragment = null;
		float nearestDistanceSquared = float.MaxValue;
		Vector2 pickupCenter = Player.PickupArea.GlobalPosition;

		foreach (Area2D overlappingArea in Player.PickupArea.GetOverlappingAreas())
		{
			if (overlappingArea.Owner is not Fragment fragment) continue;
			if (!fragment.CanBePickedUp) continue;

			float distSq = pickupCenter.DistanceSquaredTo(fragment.GlobalPosition);
			if (distSq < nearestDistanceSquared)
			{
				nearestDistanceSquared = distSq;
				nearestFragment = fragment;
			}
		}

		foreach (Node fragmentNode in GetTree().GetNodesInGroup(Fragment.PickupGroupName))
		{
			if (fragmentNode is not Fragment fragment) continue;
			if (!fragment.CanBePickedUp) continue;

			CollisionShape2D pickupSensorCollision = fragment.PickupSensor.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
			if (pickupSensorCollision?.Shape is not CircleShape2D sensorCircle)
				continue;

			float sensorRadius = sensorCircle.Radius * Mathf.Max(
				Mathf.Abs(fragment.PickupSensor.GlobalScale.X),
				Mathf.Abs(fragment.PickupSensor.GlobalScale.Y)
			);

			float distanceSquared = pickupCenter.DistanceSquaredTo(fragment.PickupSensor.GlobalPosition);
			float totalRadius = pickupRadius + sensorRadius;
			if (distanceSquared > totalRadius * totalRadius) continue;
			if (distanceSquared >= nearestDistanceSquared) continue;

			nearestDistanceSquared = distanceSquared;
			nearestFragment = fragment;
		}

		return nearestFragment;
	}

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
		if (Player.HeldFragment != null) return;

		Area2D pickupArea = Player.PickupArea;
		if (pickupArea == null)
		{
			GD.PushWarning("TryCollectFragment: Called by a non-player entity.");
			_canPickup = false;
			return;
		}

		Fragment fragmentToCollect = FindNearestPickupableFragment();
		if (fragmentToCollect == null) return;

		Player.HeldFragment = fragmentToCollect;
		fragmentToCollect.Collect(Player);
	}

	private void ThrowHeldFragment()
	{
		if (Player.HeldFragment == null || !Player.HeldFragment.CanBeThrown) return;

		Fragment heldFragment = Player.HeldFragment;
		Player.HeldFragment = null;

		Vector2 throwVelocity = new(
			Player.FacingDirection * Player.FragmentThrowSpeed,
			-Player.FragmentThrowUpwardSpeed
		);

		heldFragment.Throw(Player, throwVelocity);
	}

	private void HandleFragmentInteraction()
	{
		if (!Input.IsActionJustPressed("Interact")) return;

		if (Player.HeldFragment != null)
		{
			ThrowHeldFragment();
			return;
		}

		TryCollectFragment();
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

	private void UpdateFragmentHighlight()
	{
		Fragment nearest = Player.HeldFragment == null ? FindNearestPickupableFragment() : null;

		if (_highlightedFragment != nearest)
		{
			if (IsInstanceValid(_highlightedFragment))
				_highlightedFragment.DisableOutline();
			_highlightedFragment = nearest;
		}

		if (IsInstanceValid(_highlightedFragment))
			_highlightedFragment.EnableOutline();
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
		UpdateFragmentHighlight();
		HandleFragmentInteraction();
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
