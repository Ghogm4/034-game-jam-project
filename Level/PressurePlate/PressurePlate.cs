using Godot;
using System;

public partial class PressurePlate : AnimatableBody2D
{
    [Signal] public delegate void PressedEventHandler();
    [Signal] public delegate void ReleasedEventHandler();

    [Export] public float PressDepth = 4.0f;
    [Export] public float PressDuration = 0.1f;
    [Export] public float ReleaseDuration = 0.2f;

    private Area2D DetectArea => field ??= GetNode<Area2D>("%DetectArea");
    private int BodyCount
    {
        get => field;
        set
        {
            if (field == 0 && value > 0)
            {
                PlayPressAnimation();
            }
            else if (field > 0 && value == 0)
            {
                PlayReleaseAnimation();
            }
            field = Mathf.Max(value, 0);
        }
    } = 0;
    private Tween _currentTween;
    private float _initialY;

    public override void _Ready()
    {
        _initialY = Position.Y;
        DetectArea.BodyEntered += OnBodyEntered;
        DetectArea.BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is PressurePlate) return;
        BodyCount++;
    }
    private void OnBodyExited(Node2D body)
    {
        if (body is PressurePlate) return;
        BodyCount--;
    }

    private void PlayPressAnimation()
    {
        _currentTween?.Kill();
        _currentTween = CreateTween();
        _currentTween.TweenProperty(this, "position:y", _initialY + PressDepth, PressDuration)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Sine);
        _currentTween.TweenCallback(Callable.From(() => EmitSignal(SignalName.Pressed)));
    }

    private void PlayReleaseAnimation()
    {
        _currentTween?.Kill();
        _currentTween = CreateTween();
        _currentTween.TweenProperty(this, "position:y", _initialY, ReleaseDuration)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Elastic);
        _currentTween.TweenCallback(Callable.From(() => EmitSignal(SignalName.Released)));}
}
