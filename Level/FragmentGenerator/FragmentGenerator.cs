using Godot;
using System;

public partial class FragmentGenerator : Marker2D
{
	[Export] public PackedScene FragmentScene = null;
	[Export] public bool GenerateAtStart = true;
	private bool _hasGenerated = false;
	private Fragment _fragment = null;
	public override void _Ready()
	{
		if (GenerateAtStart)
		{
			GenerateFragment();
		}
	}
	public void GenerateFragment()
	{
		if (_hasGenerated) return;
		_hasGenerated = true;

		if (FragmentScene == null)
		{
			GD.PushError("FragmentScene is not set in FragmentGenerator.");
			return;
		}
		Fragment fragment = FragmentScene.Instantiate<Fragment>();
		if (fragment == null) return;

		AddChild(fragment);
		fragment.GlobalPosition = GlobalPosition;
		_fragment = fragment;
		fragment.Connect(Fragment.SignalName.Collected, Callable.From<Fragment>(OnCollected), (uint)ConnectFlags.OneShot);
		fragment.Connect(Fragment.SignalName.TreeExiting, Callable.From(OnCollected), (uint)ConnectFlags.OneShot);
	}
	private void OnCollected(Fragment _)
	{
		_hasGenerated = false;
		_fragment = null;
	}
	private void OnCollected()
	{
		_hasGenerated = false;
		_fragment = null;
	}
}
