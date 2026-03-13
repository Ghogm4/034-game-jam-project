using Godot;
using System;

public partial class CupHandle : Fragment
{
	[Export] public float KickSpeed = 760.0f;
	[Export] public float KickUpwardSpeed = 80.0f;
	[Export] public float MinKickMoveInput = 0.2f;
	[Export] public float MaxKickNormalAbsY = 0.65f;
    private Area2D KickSensor => field ??= GetNode<Area2D>("KickSensor");
    public override void _Ready()
    {
        KickSensor.BodyEntered += TryKick;
    }

	private void TryKick(Node2D body)
	{
        if (body is not Player player) return;
		if (!CanBePickedUp) return;
		if (player.HeldFragment == this) return;
		if (StateTree.CurrentState?.Name == "Held" || StateTree.CurrentState?.Name == "Floating") return;
		if (Mathf.Abs(player.MoveInput) < MinKickMoveInput) return;

		float kickDirection = Mathf.Sign(player.MoveInput);
		if (Mathf.IsZeroApprox(kickDirection)) return;
		if (kickDirection != player.FacingDirection) return;

		Vector2 contactNormal = (GlobalPosition - player.GlobalPosition).Normalized();
		if (Mathf.IsZeroApprox(contactNormal.LengthSquared())) return;
		if (Mathf.Abs(contactNormal.Y) > MaxKickNormalAbsY) return;
		if (Mathf.Sign(contactNormal.X) != kickDirection) return;

		LinearVelocity = new Vector2(kickDirection * KickSpeed, -KickUpwardSpeed);
		return;
	}
}
