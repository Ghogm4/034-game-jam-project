using Godot;
using System;

public partial class CupHandle : Fragment
{
	[Export] public float KickSpeed = 760.0f;
	[Export] public float KickUpwardSpeed = 80.0f;
	[Export] public float MinKickMoveInput = 0.2f;
	[Export] public float MaxKickNormalAbsY = 0.65f;
    private Area2D KickSensor => field ??= GetNode<Area2D>("KickSensor");

	public override void _PhysicsProcess(double delta)
	{
		foreach (Node2D body in KickSensor.GetOverlappingBodies())
		{
            GD.Print($"CupHandle detected overlapping body: {body.Name}");
			if (body is not Player player) continue;
			if (TryKick(player))
				break;
		}
	}

	private bool TryKick(Player player)
	{
        GD.Print($"Attempting to kick {Name} by player {player.Name} with MoveInput {player.MoveInput} and FacingDirection {player.FacingDirection}");
		if (!CanBePickedUp) return false;
		if (player.HeldFragment == this) return false;
		if (StateTree.CurrentState?.Name == "Held" || StateTree.CurrentState?.Name == "Floating") return false;
		if (Mathf.Abs(player.MoveInput) < MinKickMoveInput) return false;

		float kickDirection = Mathf.Sign(player.MoveInput);
		if (Mathf.IsZeroApprox(kickDirection)) return false;
		if (kickDirection != player.FacingDirection) return false;

		Vector2 contactNormal = (GlobalPosition - player.GlobalPosition).Normalized();
		if (Mathf.IsZeroApprox(contactNormal.LengthSquared())) return false;
		if (Mathf.Abs(contactNormal.Y) > MaxKickNormalAbsY) return false;
		if (Mathf.Sign(contactNormal.X) != kickDirection) return false;

		Throw(player, new Vector2(kickDirection * KickSpeed, -KickUpwardSpeed));
		return true;
	}
}
