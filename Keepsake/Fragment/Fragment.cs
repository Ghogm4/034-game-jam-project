using Godot;
using System;

public partial class Fragment : Area2D
{
	[Export] public float FloatAmplitude = 50.0f;
	[Export] public float FloatFrequency = 3.0f;
	private float _elapsedTime = 0.0f;
	private Vector2 _initialPosition;
	private bool _isCollected = false;
	public override void _Ready()
	{
		_initialPosition = GlobalPosition;
	}
    public override void _Process(double delta)
    {
		if (_isCollected) return;
		
        _elapsedTime += (float)delta;
		Vector2 position = GlobalPosition;
		position.Y = _initialPosition.Y + Mathf.Sin(_elapsedTime * FloatFrequency) * FloatAmplitude;
		GlobalPosition = position;
	}
	public void Collect()
	{
		CallDeferred(MethodName.SetMonitoring, false);
		_isCollected = true;
		_elapsedTime = 0.0f;
	}
	public void Drop()
	{
		CallDeferred(MethodName.SetMonitoring, true);
		_isCollected = false;
		_elapsedTime = 0.0f;
		_initialPosition = GlobalPosition;
	}
}
