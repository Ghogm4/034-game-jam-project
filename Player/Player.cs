using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float MoveSpeed = 300.0f;
	[Export] public bool IsInCutScene = false;
	[Export] public bool CutSceneJumped = false;
	[Export] public float CutSceneMoveDir = 0.0f;
	[Export] public float GroundAcceleration = 1600.0f;
	[Export] public float GroundDeceleration = 1800.0f;
	[Export] public float AirAcceleration = 900.0f;
	[Export] public float AirDeceleration = 700.0f;
	[Export] public float JumpVelocity = 430.0f;
	[Export] public float CoyoteTime = 0.1f;
	[Export] public float JumpBufferTime = 0.1f;
	[Export] public float GravityScale = 1.0f;
	[Export] public float MaxFallSpeed = 900.0f;
	[Export] public float IdleSpeedThreshold = 0f;
	[Export] public float TiltAngleDegrees = 7.0f;
	[Export] public float TiltSmoothing = 10.0f;
	[Export] public float VisualScaleSmoothing = 14.0f;
	[Export] public float ImpactVisualScaleSmoothing = 18.0f;
	[Export] public Vector2 IdleSquashScale = new(0.97f, 1.03f);
	[Export] public float IdleSquashSpeed = 3.0f;
	[Export] public float MoveSquashStrength = 0.1f;
	[Export] public float MoveSquashSpeed = 10.0f;
	[Export] public Vector2 MidAirRiseScale = new(0.92f, 1.08f);
	[Export] public Vector2 MidAirFallScale = new(1.08f, 0.92f);
	[Export] public float MidAirVisualVelocityRange = 700.0f;
	[Export] public Vector2 JumpStretchScale = new(0.92f, 1.1f);
	[Export] public Vector2 LandingSquashScale = new(1.12f, 0.88f);
	[Export] public float LandingSquashVelocityRange = 900.0f;
	[Export] public float FragmentXAmplitude = 900f;
	[Export] public float FragmentYAmplitude = 0.07f;
	[Export] public float FragmentHoldLerpSpeed = 0.5f;
	[Export] public float FragmentDefaultHoldDistance = 50f;
	[Export] public float FragmentThrowSpeed = 900f;
	[Export] public float FragmentThrowUpwardSpeed = 120f;
	public Node2D Visual => field ??= GetNode<Node2D>("Visual");
	public Area2D PickupArea => field ??= GetNode<Area2D>("%PickupArea");
	public Vector2 BaseVisualScale { get; set; } = Vector2.One;
	public Vector2 ImpactVisualScale { get; set; } = Vector2.One;
	public Fragment HeldFragment { get; set; } = null;
	public float MoveInput { get; set; } = 0.0f;
	public int FacingDirection { get; set; } = 1;
	public float VisualTime { get; set; } = 0.0f;
	public float CoyoteTimer { get; set; } = 0.0f;
	public float JumpBufferTimer { get; set; } = 0.0f;
	public float LandingImpactSpeed { get; set; } = 0.0f;
	public float CutSceneTargetXPos { get; set; } = 0.0f;
	public ulong CutSceneMoveRequestId { get; set; } = 0;

	private bool IsArrived(float currentXPos, float targetXPos, float moveDir)
	{
		if (moveDir > 0.0f)
		{
			return currentXPos >= targetXPos;
		}
		else if (moveDir < 0.0f)
		{
			return currentXPos <= targetXPos;
		}
		else
		{
			return true;
		}
	}
	private async void DetectCutSceneArrival(float xPos, ulong requestId)
	{
		while (requestId == CutSceneMoveRequestId)
		{
			bool arrived = IsArrived(GlobalPosition.X, xPos, CutSceneMoveDir);
			if (arrived)
			{
				CutSceneMoveDir = 0f;
				break;
			}

			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		}
	}

	public void CutSceneMove(float xPos)
	{
		CutSceneTargetXPos = xPos;
		CutSceneMoveRequestId++;
		CutSceneMoveDir = Mathf.Sign(xPos - GlobalPosition.X);
		DetectCutSceneArrival(xPos, CutSceneMoveRequestId);
	}
}
