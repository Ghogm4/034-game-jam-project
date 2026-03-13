using Godot;

public partial class InGameUI : CanvasLayer
{
    private Control PauseMenu => field ??= GetNode<Control>("%PauseMenu");
    private ResponsiveButton ResumeButton => field ??= GetNode<ResponsiveButton>("%ResumeButton");
    private ResponsiveButton RestartButton => field ??= GetNode<ResponsiveButton>("%RestartButton");
    private ResponsiveButton ExitButton => field ??= GetNode<ResponsiveButton>("%ExitButton");

    private bool _menuOpen = false;
    private bool _pausedByMenu = false;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        ResumeButton.Pressed += OnResumePressed;
        RestartButton.Pressed += OnRestartPressed;
        ExitButton.Pressed += OnExitPressed;
        HideMenu();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!IsEscToggleEvent(@event)) return;
        if (!CanTogglePauseMenu()) return;

        if (_menuOpen)
        {
            HideMenu();
        }
        else
        {
            ShowMenu();
        }

        GetViewport().SetInputAsHandled();
    }

    public override void _ExitTree()
    {
        // Safety: never leave the tree paused if this UI gets freed while menu is open.
        if (_pausedByMenu && GetTree() != null)
        {
            GetTree().Paused = false;
        }
    }
    private void RefreshButtonScales()
    {
        ResumeButton.RefreshScale();
        RestartButton.RefreshScale();
        ExitButton.RefreshScale();
    }
    private void ShowMenu()
    {
        if (_menuOpen) return;

        ResetPauseButtonVisuals();
        PauseMenu.Visible = true;
        _menuOpen = true;
        RefreshButtonScales();
        if (!GetTree().Paused)
        {
            GetTree().Paused = true;
            _pausedByMenu = true;
        }
    }

    private void HideMenu()
    {
        PauseMenu.Visible = false;
        _menuOpen = false;

        if (_pausedByMenu)
        {
            GetTree().Paused = false;
            _pausedByMenu = false;
        }
    }

    private void ResetPauseButtonVisuals()
    {
        ResumeButton.RefreshScale();
        RestartButton.RefreshScale();
        ExitButton.RefreshScale();
    }

    private static bool IsEscToggleEvent(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel", false, true))
        {
            return true;
        }

        if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.Escape)
        {
            return true;
        }

        return false;
    }

    private bool CanTogglePauseMenu()
    {
        var currentScene = GetTree().CurrentScene;
        if (currentScene == null) return true;

        // Never open in start menu scene.
        if (currentScene.Name == "StartMenu") return false;
        if (currentScene.SceneFilePath == "res://UI/StartMenu/StartMenu.tscn") return false;

        return true;
    }

    private void OnResumePressed()
    {
        HideMenu();
    }

    private void OnRestartPressed()
    {
        HideMenu();
        GameManager.Instance?.LoadPhase((SceneManager.TransitionColor)0, 0.5f, 1.0f, 0.5f);
    }

    private void OnExitPressed()
    {
        HideMenu();
        GetTree().ChangeSceneToFile("res://UI/StartMenu/StartMenu.tscn");
    }
}
