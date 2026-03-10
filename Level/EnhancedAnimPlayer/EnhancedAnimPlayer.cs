using Godot;
using System;

public partial class EnhancedAnimPlayer : AnimationPlayer
{
	public void PlayAnimation(StringName name, float customBlendTime = -1f, float customSpeed = 1f, bool fromEnd = false)
	{
		Play(name, customBlendTime, customSpeed, fromEnd);
	}
	public void PauseAnimation()
	{
		Pause();
	}
	public void StopAnimation()
	{
		Stop();
	}
}
