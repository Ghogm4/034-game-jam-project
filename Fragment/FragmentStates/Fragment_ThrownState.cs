using Godot;
using System;

public partial class Fragment_ThrownState : Fragment_FragmentState
{
	private bool _hasLeftPlayer = false;

	protected override void Enter()
	{
		_hasLeftPlayer = false;
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

		AudioManager.Instance.PlaySFX("Throw");
	}

	private bool IsOverlappingPlayer()
	{
		if (Fragment.ThrowOwner == null) return false;

		var spaceState = Fragment.GetWorld2D().DirectSpaceState;
		var query = new PhysicsShapeQueryParameters2D();
		query.CollideWithBodies = true;
		query.CollisionMask = Fragment.ThrowOwner.CollisionLayer;
		query.Transform = Fragment.GlobalTransform;

		if (Fragment.PhysicsCollisionShape is CollisionShape2D collisionShape)
		{
			query.Shape = collisionShape.Shape;
			query.Transform = new Transform2D(Fragment.GlobalTransform.Rotation, Fragment.GlobalTransform.Origin + collisionShape.Position.Rotated(Fragment.GlobalTransform.Rotation));
		}
		else if (Fragment.PhysicsCollisionShape is CollisionPolygon2D collisionPolygon)
		{
			var convex = new ConvexPolygonShape2D();
			convex.Points = collisionPolygon.Polygon;
			query.Shape = convex;
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

		if (!_hasLeftPlayer)
		{
			if (IsOverlappingPlayer()) return;
			_hasLeftPlayer = true;
			SetPhysicsCollisionEnabled(true);
			SetPickupEnabled(true);
			return;
		}

		if (IsOverlappingPlayer()) return;
		RestorePlayerCollision(Fragment.ThrowOwner);
		Fragment.ThrowOwner = null;
	}
}