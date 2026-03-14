using Godot;
using System;
using System.Collections.Frozen;

public partial class Fragment : RigidBody2D
{
	public const string PickupGroupName = "pickup_fragments";
	[Signal] public delegate void CollectedEventHandler(Fragment frag);
	[Signal] public delegate void ThrownEventHandler();
	[Export] public float FloatAmplitude = 50.0f;
	[Export] public float FloatFrequency = 3.0f;
	[Export] public float ThrownGravityScale = 1.0f;
	[Export] public float CollisionEnableDistanceFromPlayer = 96.0f;
	[Export] public string FragmentName = "Fragment";
	[Export] public float ModifiedJumpVelocity = 900f;
	[Export] public bool ProceedWhenCollected = false;
	[Export] public bool CanBeThrown = true;
	public Sprite2D VisualSprite => field ??= GetNode<Sprite2D>("Sprite2D");
	public Node2D PhysicsCollisionShape
	{
		get
		{
			if (field != null) return field;
			var collisionPolygon = GetNodeOrNull<CollisionPolygon2D>("CollisionPolygon2D");
			if (collisionPolygon != null) return field = collisionPolygon;
			return field = GetNode<CollisionShape2D>("CollisionShape2D");
		}
	}
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
	private static readonly Shader OutlineShader = GD.Load<Shader>("res://Fragment/OutlineShader.gdshader");
	private float _playerOriginalJumpVelocity = 0.0f;
	public void EnableOutline()
	{
		if (VisualSprite.Material is ShaderMaterial mat && mat.Shader == OutlineShader) return;
		var material = new ShaderMaterial { Shader = OutlineShader };
		material.SetShaderParameter("width", 13.0f);
		material.SetShaderParameter("outline_color", Colors.White);
		VisualSprite.Material = material;
	}

	public void DisableOutline()
	{
		if (VisualSprite.Material is not ShaderMaterial mat || mat.Shader != OutlineShader) return;
		VisualSprite.Material = null;
	}
	public override void _Ready()
	{
		AddToGroup(PickupGroupName);
		BodyEntered += OnBodyEntered;
	}
	//protected virtual void CollectBehavior() {}
	public void Collect(Player player)
	{
		PendingHolder = player;
		StateTree.CurrentState?.AskTransit("Held");
		_playerOriginalJumpVelocity = player.JumpVelocity;
		player.JumpVelocity = ModifiedJumpVelocity;
		EmitSignal(SignalName.Collected, this);
		if (ProceedWhenCollected)
		{
			AudioManager.Instance.PlayBGM("empty",3f);
			GameManager.Instance.ProceedPhase(SceneManager.TransitionColor.Black,3f,3f,1f);
		}
		//CollectBehavior();
	}

	public void Throw(Player player, Vector2 throwVelocity)
	{
		PendingThrowOwner = player;
		PendingThrowVelocity = throwVelocity;
		StateTree.CurrentState?.AskTransit("Thrown");
		player.JumpVelocity = _playerOriginalJumpVelocity;
		EmitSignal(SignalName.Thrown);
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
