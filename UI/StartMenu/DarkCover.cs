using Godot;
using System;

public partial class DarkCover : ColorRect
{
	public override void _Ready()
	{
		Modulate = Modulate with { A = 1f };
		Tween tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 0f, 1.0f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.In);
	}
}
