using Godot;
using System;

public partial class Fragment_ThrownState : Fragment_FragmentState
{
	protected override void Enter()
	{
		IgnorePlayerCollision(Fragment.PendingThrowOwner);
		Fragment.Freeze = false;
		Fragment.GravityScale = Fragment.ThrownGravityScale;
		Fragment.LinearVelocity = Fragment.PendingThrowVelocity;
		Fragment.AngularVelocity = 0.0f;
		Fragment.FloatElapsedTime = 0.0f;
		Fragment.ThrowOwner = Fragment.PendingThrowOwner;
		SetPhysicsCollisionEnabled(false);
		SetPickupEnabled(false);
		Fragment.PendingThrowOwner = null;
		Fragment.PendingThrowVelocity = Vector2.Zero;
	}

	protected override void PhysicsUpdate(double delta)
	{
		if (Fragment.ThrowOwner == null) return;

		float distanceToPlayer = Fragment.GlobalPosition.DistanceTo(Fragment.ThrowOwner.GlobalPosition);
		if (distanceToPlayer < Fragment.CollisionEnableDistanceFromPlayer) return;

		RestorePlayerCollision(Fragment.ThrowOwner);
		SetPhysicsCollisionEnabled(true);
		SetPickupEnabled(true);
		Fragment.ThrowOwner = null;
	}
}