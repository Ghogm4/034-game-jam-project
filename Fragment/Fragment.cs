using Godot;
using System;
using System.Collections.Frozen;

public partial class Fragment : RigidBody2D
{
	public const string PickupGroupName = "pickup_fragments";
	[Signal] public delegate void CollectedEventHandler();
	[Export] public float FloatAmplitude = 50.0f;
	[Export] public float FloatFrequency = 3.0f;
	[Export] public float ThrownGravityScale = 1.0f;
	[Export] public float CollisionEnableDistanceFromPlayer = 96.0f;
	[Export] public string FragmentName = "Fragment";

	public Sprite2D VisualSprite => field ??= GetNode<Sprite2D>("Sprite2D");
	public CollisionShape2D PhysicsCollisionShape => field ??= GetNode<CollisionShape2D>("CollisionShape2D");
	public Area2D PickupSensor => field ??= GetNode<Area2D>("PickupSensor");
	public StateTree StateTree => field ??= GetNode<StateTree>("StateTree");
	public bool CanBePickedUp { get; internal set; } = true;
	public Player PendingHolder { get; internal set; } = null;
	public Player PendingThrowOwner { get; internal set; } = null;
	public Vector2 PendingThrowVelocity { get; internal set; } = Vector2.Zero;
	public float FloatElapsedTime { get; internal set; } = 0.0f;
	public Vector2 FloatingAnchorPosition { get; internal set; } = Vector2.Zero;
	public Player ThrowOwner { get; internal set; } = null;
	private RecipeTable RecipeTable => field ??= GetTree().CurrentScene.GetNode<RecipeTable>("%RecipeTable");
	public override void _Ready()
	{
		AddToGroup(PickupGroupName);
		BodyEntered += OnBodyEntered;
	}
	protected virtual void CollectBehavior() {}
	public void Collect(Player player)
	{
		PendingHolder = player;
		StateTree.CurrentState?.AskTransit("Held");
		EmitSignal(SignalName.Collected);
		CollectBehavior();
	}

	public void Throw(Player player, Vector2 throwVelocity)
	{
		PendingThrowOwner = player;
		PendingThrowVelocity = throwVelocity;
		StateTree.CurrentState?.AskTransit("Thrown");
	}

	public void ResetToFloating()
	{
		StateTree.CurrentState?.AskTransit("Floating");
	}

	// public void PrepareForCrafting()
	// {
	// 	SetDeferred(PropertyName.Freeze, true);
	// 	Visible = false;
	// 	PhysicsCollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
	// 	PickupSensor.SetDeferred(Area2D.PropertyName.Monitoring, false);
	// 	PickupSensor.SetDeferred(Area2D.PropertyName.Monitorable, false);
	// }

	public void OnBodyEntered(Node body)
	{
		if (body is Fragment frag)
		{
			RecipeTable.TryCraft(this, frag);
		}
	}
}
