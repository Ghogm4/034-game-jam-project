using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

public partial class SceneManager : Node2D
{
	public static SceneManager Instance { get; private set ;}

	public enum TransitionColor
	{
		Black,
		White
	}

	[Export] public Godot.Collections.Dictionary<GameManager.GamePhase, PackedScene> scenes = new Godot.Collections.Dictionary<GameManager.GamePhase, PackedScene>();
	public override void _Ready()
	{
		if (Instance != null)
		{
			GD.PrintErr("Multiple instances of SceneManager detected. This should not happen.");
			QueueFree();
			return;
		}
		Instance = this;

		rect.Visible = false;
	}

	[Export] public ColorRect rect;

	public async void TransitionToScene(PackedScene scene, TransitionColor color, float fadeIn = 0.5f, float fadeOut = 0.5f, float sustain = 0f)
	{
		rect.Color = color == TransitionColor.Black ? Colors.Black : Colors.White;
		rect.Modulate = new Color(rect.Modulate.R, rect.Modulate.G, rect.Modulate.B, 0f);
		rect.Visible = true;

		await FadeOut(fadeOut);
		GetTree().ChangeSceneToPacked(scene);
		await ToSignal(GetTree().CreateTimer(sustain), "timeout");
		await FadeIn(fadeIn);

		rect.Visible = false;
	}

	public async Task FadeOut(float duration)
	{
		if (GameManager.Instance.IsLeftHospital)
		{
			AudioManager.Instance.PlaySFX("Door Open Reverb");
		}
		Tween tween = CreateTween();
		tween.TweenProperty(rect, "modulate:a", 1f, duration);
		await ToSignal(tween, "finished");
	}

	public async Task FadeIn(float duration)
	{
		if (GameManager.Instance.IsLeftHospital)
		{
			AudioManager.Instance.PlaySFX("Door Close Reverb");
		}
		Tween tween = CreateTween();
		tween.TweenProperty(rect, "modulate:a", 0f, duration);
		await ToSignal(tween, "finished");
	}

	public async void ChangeScene(GameManager.GamePhase phase, TransitionColor color, float fadeIn = 0.5f, float fadeOut = 0.5f, float sustain = 0f)
	{
		scenes.TryGetValue(phase, out PackedScene scene);
		if (scene == null)
		{
			GD.PrintErr($"Scene for phase {phase} is not defined.");
			return;
		}
		TransitionToScene(scene, color, fadeIn, fadeOut, sustain);
	}
}
