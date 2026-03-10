using Godot;
using System;

[GlobalClass]
public partial class SFXPlayer : Node
{
	public void PlaySFX(string name)
	{
		AudioManager.Instance.PlaySFX(name);
	}  
}
