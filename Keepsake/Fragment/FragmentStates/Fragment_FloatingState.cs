using Godot;
using System;

public partial class Fragment_FloatingState : Fragment_FragmentState
{
	protected override void Enter()
	{
		RestorePlayerCollision(Fragment.ThrowOwner);
		RestorePlayerCollision(Fragment.PendingHolder);
		RestorePlayerCollision(Fragment.PendingThrowOwner);

		Fragment.Freeze = true;
		Fragment.GravityScale = 0.0f;
		Fragment.LinearVelocity = Vector2.Zero;
		Fragment.AngularVelocity = 0.0f;
		Fragment.FloatingAnchorPosition = Fragment.GlobalPosition;
		Fragment.FloatElapsedTime = 0.0f;
		Fragment.PendingHolder = null;
		Fragment.PendingThrowOwner = null;
		Fragment.PendingThrowVelocity = Vector2.Zero;
		Fragment.ThrowOwner = null;
		SetPhysicsCollisionEnabled(false);
		SetPickupEnabled(true);
	}

	protected override void FrameUpdate(double delta)
	{
		Fragment.FloatElapsedTime += (float)delta;
		Vector2 position = Fragment.GlobalPosition;
		position.Y = Fragment.FloatingAnchorPosition.Y + Mathf.Sin(Fragment.FloatElapsedTime * Fragment.FloatFrequency) * Fragment.FloatAmplitude;
		Fragment.GlobalPosition = position;
	}
}