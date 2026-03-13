using Godot;
using System;

public partial class StartMenu : Control
{
	private ResponsiveButton StartButton => field ??= GetNode<ResponsiveButton>("%StartButton");
	private ResponsiveButton CGListButton => field ??= GetNode<ResponsiveButton>("%CGListButton");
	private HBoxContainer FirstButtons => field ??= GetNode<HBoxContainer>("%FirstButtons");

	private Control CGGallery => field ??= GetNode<Control>("%CGGallery");
	private ResponsiveButton KeepsakeDisplay => field ??= GetNode<ResponsiveButton>("%KeepsakeDisplay");
	private ResponsiveButton LeftButton => field ??= GetNode<ResponsiveButton>("%LeftButton");
	private ResponsiveButton RightButton => field ??= GetNode<ResponsiveButton>("%RightButton");
	private Node2D BackGrounds => field ??= GetNodeOrNull<Node2D>("BackGrounds");

	private int _currentIndex = 0;

	private readonly string[] _keepsakeTexturePaths = new string[]
	{
		"res://Colors/Rain/purpleUm.png",
		"res://Colors/pieces/cup.png", 
		"res://Colors/pieces/Picnic/picnicStateFinal.png",
		"res://Colors/pieces/flower/flowerStateFinal.png"
	};

	private Vector2 _firstButtonsOriginalPos;
	private Vector2 _keepsakeOriginalPos;
	private Tween _menuTween;
	private Tween _keepsakeTween;
	private bool _isTransitioning = false;

	[Export] public float TransitionDuration = 0.5f;
	[Export] public float TransitionOffset = 1200f;
	[Export] public float FadeOutDelay = 0.3f;
	[Export] public float FadeOutDuration = 0.2f;
	[Export] public float ParallaxSpeedNearX = -54.0f;
	[Export] public float ParallaxSpeedMidX = -36.0f;
	[Export] public float ParallaxSpeedFarX = -18.0f;

	public override void _Ready()
	{
		StartButton.Pressed += OnStartButtonPressed;
		CGListButton.Pressed += OnCGListButtonPressed;
		
		LeftButton.Pressed += OnLeftButtonPressed;
		RightButton.Pressed += OnRightButtonPressed;
		KeepsakeDisplay.Pressed += OnKeepsakePressed;

		CGGallery.Visible = false;
		SetupTitleParallaxLoop();
		CallDeferred(MethodName.StoreOriginalPositions);

		AudioManager.Instance.PlayBGM("empty", 1f);
	}

	private void SetupTitleParallaxLoop()
	{
		if (BackGrounds == null) return;

		var near = BackGrounds.GetNodeOrNull<Parallax2D>("Parallax2D2");
		var mid = BackGrounds.GetNodeOrNull<Parallax2D>("Buildings2");
		var far = BackGrounds.GetNodeOrNull<Parallax2D>("Buildings3");

		if (near != null) near.Set("autoscroll", new Vector2(ParallaxSpeedNearX, 0.0f));
		if (mid != null) mid.Set("autoscroll", new Vector2(ParallaxSpeedMidX, 0.0f));
		if (far != null) far.Set("autoscroll", new Vector2(ParallaxSpeedFarX, 0.0f));
	}

	private void StoreOriginalPositions()
	{
		_firstButtonsOriginalPos = FirstButtons.Position;
		_keepsakeOriginalPos = KeepsakeDisplay.Position;
	}

	private void OnStartButtonPressed()
	{
		GD.Print("Start Game");
		GameManager.Instance.SetCurrentGameState(GameManager.GameState.Playing);
		GameManager.Instance.LoadPhase(SceneManager.TransitionColor.Black, 0.5f, 0.5f, 0f);
	}

	private void OnCGListButtonPressed()
	{
		GameManager.Instance.SetCurrentGameState(GameManager.GameState.Cutscene);
		
		StopActiveTweens();
		_isTransitioning = true;

		_currentIndex = 0;
		CGGallery.Visible = true;

		CGGallery.Position = new Vector2(GetViewportRect().Size.X, 0);

		LeftButton.Disabled = false;
		RightButton.Disabled = false;
		
		KeepsakeDisplay.Position = _keepsakeOriginalPos;
		KeepsakeDisplay.Modulate = Colors.White;
		KeepsakeDisplay.Visible = true;
		
		UpdateGallery();

		_menuTween = CreateTween();
		_menuTween.SetParallel(true);
		_menuTween.SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);
		
		_menuTween.TweenProperty(FirstButtons, "position:x", -FirstButtons.Size.X - 800, 1.0f);
		
		_menuTween.TweenProperty(CGGallery, "position:x", 0.0f, 1.0f);
		_menuTween.Chain().TweenCallback(Callable.From(() => _isTransitioning = false));
	}

	private void OnLeftButtonPressed()
	{
		if (_isTransitioning && IsInstanceValid(_menuTween))
		{
			CompleteMenuEnterImmediately();
		}

		if (_isTransitioning) return;

		if (_currentIndex == 0)
		{
			StopActiveTweens();
			_isTransitioning = true;

			_menuTween = CreateTween();
			_menuTween.SetParallel(true);
			_menuTween.SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);

			_menuTween.TweenProperty(FirstButtons, "position:x", _firstButtonsOriginalPos.X, 1.0f);

			_menuTween.TweenProperty(CGGallery, "position:x", GetViewportRect().Size.X, 1.0f);

			_menuTween.Chain().TweenCallback(Callable.From(() => 
			{
				CGGallery.Visible = false;
				KeepsakeDisplay.Position = _keepsakeOriginalPos;
				RightButton.Visible = true;
				_isTransitioning = false;
			}));
		}
		else
		{
			_currentIndex--;
			AnimateKeepsakeSwitch(-1);
		}
	}

	private void OnRightButtonPressed()
	{
		if (_isTransitioning && IsInstanceValid(_menuTween))
		{
			CompleteMenuEnterImmediately();
		}

		if (_isTransitioning) return;

		if (_currentIndex < _keepsakeTexturePaths.Length - 1)
		{
			_currentIndex++;
			AnimateKeepsakeSwitch(1);
		}
	}

	private void AnimateKeepsakeSwitch(int direction)
	{
		StopActiveKeepsakeTween();
		_isTransitioning = true;

		float offset = TransitionOffset;
		
		float exitDestX = _keepsakeOriginalPos.X - (direction * offset);
		float enterStartX = _keepsakeOriginalPos.X + (direction * offset);
		Vector2 enterPos = new Vector2(enterStartX, _keepsakeOriginalPos.Y);

		LeftButton.Disabled = true;
		RightButton.Disabled = true;

		var oldTexture = KeepsakeDisplay.Icon;
		
		var exitingKeepsake = (Button)KeepsakeDisplay.Duplicate();
		CGGallery.AddChild(exitingKeepsake);
		exitingKeepsake.Position = KeepsakeDisplay.Position;
		exitingKeepsake.Icon = oldTexture;

		UpdateGallery(); 
		KeepsakeDisplay.Position = enterPos;

		_keepsakeTween = CreateTween();
		_keepsakeTween.SetParallel(true);
		_keepsakeTween.SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);

		_keepsakeTween.TweenProperty(exitingKeepsake, "position:x", exitDestX, TransitionDuration);
		if (FadeOutDuration > 0)
		{
			_keepsakeTween.TweenProperty(exitingKeepsake, "modulate:a", 0.0f, FadeOutDuration).SetDelay(FadeOutDelay);
		}

		_keepsakeTween.TweenProperty(KeepsakeDisplay, "position:x", _keepsakeOriginalPos.X, TransitionDuration);

		_keepsakeTween.Chain().TweenCallback(Callable.From(() =>
		{
			 LeftButton.Disabled = false;
			 RightButton.Disabled = false;
			 exitingKeepsake.QueueFree();
			 _isTransitioning = false;
		}));
	}

	private void StopActiveTweens()
	{
		StopActiveKeepsakeTween();
		if (IsInstanceValid(_menuTween))
		{
			_menuTween.Kill();
			_menuTween = null;
		}
	}

	private void StopActiveKeepsakeTween()
	{
		if (IsInstanceValid(_keepsakeTween))
		{
			_keepsakeTween.Kill();
			_keepsakeTween = null;
		}

		LeftButton.Disabled = false;
		RightButton.Disabled = false;
	}

	private void CompleteMenuEnterImmediately()
	{
		if (!IsInstanceValid(_menuTween)) return;

		_menuTween.Kill();
		_menuTween = null;

		FirstButtons.Position = new Vector2(-FirstButtons.Size.X - 800, FirstButtons.Position.Y);
		CGGallery.Position = new Vector2(0.0f, CGGallery.Position.Y);
		_isTransitioning = false;
	}

	private void OnKeepsakePressed()
	{
		GD.Print($"Keepsake {_currentIndex} Pressed");
		if (_currentIndex == 0 && (int)GameManager.Instance.SavedGamePhase >= 5)
		{
			SceneManager.Instance.ChangeScene(GameManager.GamePhase.Cutscene_Rain, SceneManager.TransitionColor.Black, 0.5f, 0.5f, 1f);
		}
		else if (_currentIndex == 1 && (int)GameManager.Instance.SavedGamePhase >= 8)
		{
			SceneManager.Instance.ChangeScene(GameManager.GamePhase.Cutscene_Cup, SceneManager.TransitionColor.Black, 0.5f, 0.5f, 1f);
		}
		else if (_currentIndex == 2 && (int)GameManager.Instance.SavedGamePhase >= 11)
		{
			SceneManager.Instance.ChangeScene(GameManager.GamePhase.Cutscene_Picnic, SceneManager.TransitionColor.Black, 0.5f, 0.5f, 1f);
		}

		//test
		// else SceneManager.Instance.ChangeScene(GameManager.GamePhase.Hospital_1, SceneManager.TransitionColor.Black, 0.5f, 0.5f, 1f);
		
	}

	private void UpdateGallery()
	{
		var texture = GD.Load<Texture2D>(_keepsakeTexturePaths[_currentIndex]);
		KeepsakeDisplay.Icon = texture;
		if (_currentIndex == 0 && (int)GameManager.Instance.SavedGamePhase < 5
			|| _currentIndex == 1 && (int)GameManager.Instance.SavedGamePhase < 8
			|| _currentIndex == 2 && (int)GameManager.Instance.SavedGamePhase < 11
			|| _currentIndex == 3 && (int)GameManager.Instance.SavedGamePhase < 14)
		{
			KeepsakeDisplay.Icon = GD.Load<Texture2D>("res://Colors/City1/blackSquare.png");
			KeepsakeDisplay.Disabled = true;
		}
		else KeepsakeDisplay.Disabled = false;
		
		LeftButton.Visible = true; 

		RightButton.Visible = _currentIndex < _keepsakeTexturePaths.Length - 1;
	}
}
