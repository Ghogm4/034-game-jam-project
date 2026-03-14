using Godot;
using System;

public partial class Fragment_FragmentState : State
{
	protected Fragment Fragment => field ??= Owner as Fragment;

	protected void SetPhysicsCollisionEnabled(bool enabled)
	{
		if (Fragment.PhysicsCollisionShape is CollisionShape2D collisionShape)
		{
			collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, !enabled);
			return;
		}

		if (Fragment.PhysicsCollisionShape is CollisionPolygon2D collisionPolygon)
		{
			collisionPolygon.SetDeferred(CollisionPolygon2D.PropertyName.Disabled, !enabled);
			return;
		}

		GD.PushWarning($"SetPhysicsCollisionEnabled: Unsupported collision node type on fragment '{Fragment.Name}'.");
	}

	protected void SetPickupEnabled(bool enabled)
	{
		Fragment.CanBePickedUp = enabled;
	}

	protected void IgnorePlayerCollision(Player player)
	{
		if (player == null) return;

		Fragment.AddCollisionExceptionWith(player);
		player.AddCollisionExceptionWith(Fragment);
	}

	protected void RestorePlayerCollision(Player player)
	{
		if (player == null) return;

		Fragment.RemoveCollisionExceptionWith(player);
		player.RemoveCollisionExceptionWith(Fragment);
	}
}