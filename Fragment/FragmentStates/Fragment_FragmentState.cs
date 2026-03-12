using Godot;
using System;

public partial class Fragment_FragmentState : State
{
	protected Fragment Fragment => field ??= Owner as Fragment;

	protected void SetPhysicsCollisionEnabled(bool enabled)
	{
		Fragment.PhysicsCollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, !enabled);
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