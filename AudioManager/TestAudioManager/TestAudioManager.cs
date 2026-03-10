using Godot;
using System;

public partial class TestAudioManager : Node
{
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	public override void _Ready()
	{
		AnimationPlayer.Play("TestAudioManager");
	}
}
