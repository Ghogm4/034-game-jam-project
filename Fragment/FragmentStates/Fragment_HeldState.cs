using Godot;
using System;

public partial class Fragment_HeldState : Fragment_FragmentState
{
	protected override void Enter()
	{
		IgnorePlayerCollision(Fragment.PendingHolder);
		Fragment.Freeze = true;
		Fragment.GravityScale = 0.0f;
		Fragment.LinearVelocity = Vector2.Zero;
		Fragment.AngularVelocity = 0.0f;
		Fragment.FloatElapsedTime = 0.0f;
		Fragment.ThrowOwner = Fragment.PendingHolder;
		SetPhysicsCollisionEnabled(false);
		SetPickupEnabled(false);
		Fragment.PendingHolder = null;

		AudioManager.Instance.PlaySFX("Collect");
	}
}