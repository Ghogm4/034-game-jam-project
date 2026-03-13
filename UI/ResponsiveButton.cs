using Godot;
using System;
[GlobalClass]
public partial class ResponsiveButton : Button
{
    [Export] public float HoverScale = 1.1f;
    [Export] public float PressedScale = 0.95f;
    [Export] public float OriginalScale = 1.0f;
    [Export] public float Duration = 0.1f;

    private Vector2 _fromScale = Vector2.One;
    private Vector2 _targetScale = Vector2.One;
    private float _time = 0f;
    private bool _isAnimating = false;

    private bool _fromPressed = false;
    private bool _isHovered = false;
    public void RefreshScale()
    {
        SetDeferred(PropertyName.Scale, Vector2.One * OriginalScale);
    }
    public sealed override void _Ready()
    {
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        ButtonDown += OnButtonDown;
        ButtonUp += OnButtonUp;

        SetDeferred(PropertyName.PivotOffset, Size / 2);
        SetDeferred(PropertyName.Scale, Vector2.One * OriginalScale);
        ReadyBehavior();
    }

    public override void _Process(double delta)
    {
        if (!_isAnimating) return;

        _time += (float)delta;
        float t = Mathf.Clamp(_time / Duration, 0f, 1f);
        float easeT = Ease(t);
        Scale = _fromScale.Lerp(_targetScale, easeT);

        if (t >= 1f)
        {
            _isAnimating = false;
            Scale = _targetScale;
        }
    }

    protected virtual void ReadyBehavior() { }

    private void OnMouseEntered()
    {
        _isHovered = true;
        if (!ButtonPressed)
        {
            StartAnimation(Vector2.One * HoverScale);
        }
    }

    private void OnMouseExited()
    {
        _isHovered = false;
        if (!ButtonPressed)
        {
            StartAnimation(Vector2.One * OriginalScale);
        }
    }

    private void OnButtonDown()
    {
        AudioManager.Instance.PlaySFX("UI");
        StartAnimation(Vector2.One * PressedScale);
    }

    private void OnButtonUp()
    {
        if (_isHovered)
        {
            StartAnimation(Vector2.One * HoverScale);
        }
        else
        {
            StartAnimation(Vector2.One * OriginalScale);
        }
    }

    private void StartAnimation(Vector2 target)
    {
        _fromScale = Scale;
        _targetScale = target;
        _time = 0f;
        _isAnimating = true;
    }

    private float Ease(float t)
    {
        return 1 - Mathf.Pow(1 - t, 4);
    }
}
