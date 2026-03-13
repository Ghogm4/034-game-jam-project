using Godot;
using System;

[GlobalClass]
public partial class SFXPlayer : Node
{
	public void PlaySFX(string name)
	{
		AudioManager.Instance.PlaySFX(name);
	}  

	public void StopSFX(string name)
	{
		AudioManager.Instance.StopSFX(name);
	}
}
