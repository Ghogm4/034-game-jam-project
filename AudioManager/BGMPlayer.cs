using Godot;
using System;

[GlobalClass]
public partial class BGMPlayer : Node
{
	public void PlayBGM(string name, float fadeDuration = -1)
	{
		AudioManager.Instance.PlayBGM(name, fadeDuration);
	}
}
