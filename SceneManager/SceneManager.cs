using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class SceneManager : Node2D
{
	public static SceneManager Instance { get; private set ;}

	[Export] public Godot.Collections.Dictionary<GameManager.GamePhase, string> scenePaths = new Godot.Collections.Dictionary<GameManager.GamePhase, string>();
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

	public async void TransitionToScene(string scenePath, Color color, float fadeIn = 0.5f, float fadeOut = 0.5f, float sustain = 0f)
	{
		rect.Color = color;
		rect.Modulate = new Color(rect.Modulate.R, rect.Modulate.G, rect.Modulate.B, 0f);
		rect.Visible = true;

		await FadeOut(fadeOut);
		GetTree().ChangeSceneToFile(scenePath);
		await ToSignal(GetTree().CreateTimer(sustain), "timeout");
		await FadeIn(fadeIn);

		rect.Visible = false;
	}

	public async Task FadeOut(float duration)
	{
		Tween tween = CreateTween();
		tween.TweenProperty(rect, "modulate:a", 1f, duration);
		await ToSignal(tween, "finished");
	}

	public async Task FadeIn(float duration)
	{
		Tween tween = CreateTween();
		tween.TweenProperty(rect, "modulate:a", 0f, duration);
		await ToSignal(tween, "finished");
	}

	public async void ChangeScene(GameManager.GamePhase phase, Color color, float fadeIn = 0.5f, float fadeOut = 0.5f, float sustain = 0f)
	{
		scenePaths.TryGetValue(phase, out string scenePath);
		if (string.IsNullOrEmpty(scenePath))
		{
			GD.PrintErr($"Scene path for phase {phase} is not defined.");
			return;
		}
		TransitionToScene(scenePath, color, fadeIn, fadeOut, sustain);
	}
}
