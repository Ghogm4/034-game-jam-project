using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float MoveSpeed = 300.0f;
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
	[Export] public Vector2 IdleSquashScale = new(0.97f, 1.03f);
	[Export] public float IdleSquashSpeed = 3.0f;
	[Export] public float MoveSquashStrength = 0.1f;
	[Export] public float MoveSquashSpeed = 10.0f;
	[Export] public Vector2 MidAirRiseScale = new(0.92f, 1.08f);
	[Export] public Vector2 MidAirFallScale = new(1.08f, 0.92f);
	[Export] public float MidAirVisualVelocityRange = 700.0f;

	public Node2D Visual => field ??= GetNode<Node2D>("Visual");
	public Vector2 TargetVisualScale { get; set; } = Vector2.One;
	public float MoveInput { get; set; } = 0.0f;
	public int FacingDirection { get; set; } = 1;
	public float VisualTime { get; set; } = 0.0f;
	public float CoyoteTimer { get; set; } = 0.0f;
	public float JumpBufferTimer { get; set; } = 0.0f;
}
