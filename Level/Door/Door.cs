using Godot;
using System;

public partial class Door : AnimatableBody2D
{
	[Export] public float OpenSpeed = 50f;
	[Export] public float OpenHeight = 100.0f;
	[Export] public bool StartOpen = false;
	private float OpenDuration => OpenHeight / OpenSpeed;
	private Tween _currentTween;
	private float _initialY;
	public override void _Ready()
	{
		_initialY = Position.Y;
		if (StartOpen)
		{
			Position = new Vector2(Position.X, _initialY - OpenHeight);
		}
	}
	public void Open()
	{
		_currentTween?.Kill();
		_currentTween = CreateTween();
		_currentTween.TweenProperty(this, "position:y", _initialY - OpenHeight, OpenDuration)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Sine);
	}
	public void Close()
	{
		_currentTween?.Kill();
		_currentTween = CreateTween();
		_currentTween.TweenProperty(this, "position:y", _initialY, OpenDuration)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Sine);
	}
}
