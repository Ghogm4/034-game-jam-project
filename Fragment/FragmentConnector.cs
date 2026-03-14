using Godot;
using System;

public partial class FragmentConnector : Node
{
	[Export] public float LineWidth = 4.0f;
	[Export] public float MinAlpha = 0.2f;
	[Export] public float MaxAlpha = 0.5f;
	[Export] public float BlinkDuration = 0.5f;
	private bool _isDirty = true;
	private Godot.Collections.Array<Fragment> Fragments
	{
		get
		{
			if (_isDirty)
			{
				GD.Print("Refreshing fragment list in FragmentConnector...");
				field = new Godot.Collections.Array<Fragment>();
				Godot.Collections.Array<Node> nodesInGroup = IsInsideTree() ? GetTree().GetNodesInGroup("Fragments") : null;
				if (nodesInGroup == null || nodesInGroup.Count == 0)
				{
					GD.PushWarning("No fragments found in the scene.");
					return null;
				}
				foreach (var node in nodesInGroup)
				{
					if (node is Fragment fragment)
					{
						field.Add(fragment);
						if (fragment.IsConnected(Fragment.SignalName.Collected, Callable.From<Fragment>(Connect))) continue;

						fragment.Connect(Fragment.SignalName.Collected, Callable.From<Fragment>(Connect));
						fragment.Connect(Fragment.SignalName.TreeExiting, Callable.From(OnFragmentTreeExiting));
						fragment.Connect(Fragment.SignalName.Thrown, Callable.From(Disconnect));
						GD.Print($"Connected to fragment: {fragment.Name}");
					}
				}
				_isDirty = false;
			}
			return field;
		}
	} = null;
	public override void _Ready()
	{
		RefreshFragments();
	}

	private void OnNodeAdded(Node node)
	{
		if (node is Fragment)
			_isDirty = true;
	}

	private void RefreshFragments()
	{
		var _ = Fragments;
	}
	private void Connect(Fragment centerFragment)
	{
		GD.Print($"Connecting from fragment: {centerFragment.Name}");
		foreach (var frag in Fragments)
		{
			if (frag is not Fragment fragment) continue;
			if (fragment != centerFragment)
			{
				GD.Print($"Connecting to fragment: {fragment.Name}");
				ConnectFragments(centerFragment, fragment);
			}
		}
	}
	private async void UpdateLine(Line2D line, Fragment fragA, Fragment fragB)
	{
		while (IsInstanceValid(line) && IsInstanceValid(fragA) && IsInstanceValid(fragB) && IsInsideTree())
		{
			line.SetPointPosition(0, fragA.GlobalPosition);
			line.SetPointPosition(1, fragB.GlobalPosition);
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		if (IsInstanceValid(line) && (line?.IsInsideTree() ?? false))
		{
			line.QueueFree();
		}
	}
	private void ConnectFragments(Fragment fragA, Fragment fragB)
	{
		Line2D line = new Line2D()
		{
			DefaultColor = new Color(1, 1, 1, MinAlpha),
			Width = LineWidth
		};
		CallDeferred(MethodName.AddChild, line);
		line.Points = [fragA.GlobalPosition, fragB.GlobalPosition];
		UpdateLine(line, fragA, fragB);
		MakeLineBlink(line);
	}
	private void MakeLineBlink(Line2D line)
	{
		Tween tween = line.CreateTween();
		tween.TweenProperty(line, "default_color:a", MaxAlpha, BlinkDuration)
			.SetEase(Tween.EaseType.InOut)
			.SetTrans(Tween.TransitionType.Sine);
		tween.TweenProperty(line, "default_color:a", MinAlpha, BlinkDuration)
			.SetEase(Tween.EaseType.InOut)
			.SetTrans(Tween.TransitionType.Sine);
		tween.SetLoops();
	}
	private void OnFragmentTreeExiting()
	{
		_isDirty = true;
		CallDeferred(MethodName.RefreshFragments);
	}
	private void Disconnect()
	{
		foreach (var child in GetChildren())
		{
			if (child is not Line2D line) continue;

			line.QueueFree();
		}
	}
	private void Disconnect(Fragment _) => Disconnect();
}
