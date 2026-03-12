using Godot;
using System;

public partial class SceneChangeTest : Node2D
{
	[Export] public AnimationPlayer animPlayer;
	public override void _Ready()
	{
		GameManager.Instance.SetCurrentGamePhase(GameManager.GamePhase.Level_3);
		animPlayer.Play("test");
	}
}
