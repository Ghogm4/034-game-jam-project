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
		// Keep world collision active while thrown; only ignore the player via collision exception.
		SetPhysicsCollisionEnabled(true);
		// Allow re-pickup immediately after throw, even while still overlapping player.
		SetPickupEnabled(true);
		Fragment.PendingThrowOwner = null;
		Fragment.PendingThrowVelocity = Vector2.Zero;

		AudioManager.Instance.PlaySFX("Throw");
	}

	private bool IsOverlappingPlayer()
	{
		if (Fragment.ThrowOwner == null) return false;

		var spaceState = Fragment.GetWorld2D().DirectSpaceState;
		var query = new PhysicsShapeQueryParameters2D();
		query.CollideWithBodies = true;
		query.CollisionMask = Fragment.ThrowOwner.CollisionLayer;
		query.Margin = 0.0f;

		if (Fragment.PhysicsCollisionShape is CollisionShape2D collisionShape)
		{
			query.Shape = collisionShape.Shape;
			query.Transform = Fragment.GlobalTransform * collisionShape.Transform;
		}
		else if (Fragment.PhysicsCollisionShape is CollisionPolygon2D collisionPolygon)
		{
			var convex = new ConvexPolygonShape2D();
			convex.Points = collisionPolygon.Polygon;
			query.Shape = convex;
			query.Transform = Fragment.GlobalTransform * collisionPolygon.Transform;
		}
		else
		{
			return false;
		}

		var results = spaceState.IntersectShape(query, 8);
		foreach (var result in results)
		{
			if (result["collider"].AsGodotObject() == Fragment.ThrowOwner)
				return true;
		}
		return false;
	}

	protected override void PhysicsUpdate(double delta)
	{
		if (Fragment.ThrowOwner == null) return;

		if (IsOverlappingPlayer()) return;
		RestorePlayerCollision(Fragment.ThrowOwner);
		Fragment.ThrowOwner = null;
	}
}