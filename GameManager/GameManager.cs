using System;
using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public override void _Ready()
    {
        if (Instance != null)
        {
            GD.PrintErr("Multiple instances of GameManager detected. This should not happen.");
            QueueFree();
            return;
        }
        Instance = this;
    }

    public enum GameState
    {
        Playing,
        Paused,
        Cutscene,
    }

    public enum GamePhase
    {
        Opening,
        Cutscene_City,
        Cutscene_CarCrash,
        Hospital_1,
        Level_1,
        Cutscene_Rain,
        Hospital_2,
        Level_2,
        Cutscene_Cup,
        Hospital_3,
        Level_3,
        Cutscene_Picnic,
        Hospital_4,
        Level_4,
        Cutscene_Hospital,
        Cutscene_Finale,
        StartMenu,
    }

    public bool IsInHospital { get
        {
            return CurrentGamePhase == GamePhase.Hospital_1
                || CurrentGamePhase == GamePhase.Hospital_2
                || CurrentGamePhase == GamePhase.Hospital_3
                || CurrentGamePhase == GamePhase.Hospital_4;
        }
        private set { }
    }

    public bool IsLeftHospital { get
        {
            return CurrentGamePhase == GamePhase.Level_1
                || CurrentGamePhase == GamePhase.Level_2
                || CurrentGamePhase == GamePhase.Level_3
                || CurrentGamePhase == GamePhase.Level_4;
        }
        private set { }
    }

    public bool IsEnterCG { get
        {
            return CurrentGamePhase == GamePhase.Cutscene_Cup
                || CurrentGamePhase == GamePhase.Cutscene_Picnic
                || CurrentGamePhase == GamePhase.Cutscene_Rain;
        }
        private set { }
    }

    public GameState CurrentGameState { get; private set; } = GameState.Playing;
    public GamePhase CurrentGamePhase { get; private set; } = GamePhase.Opening;
    public GamePhase SavedGamePhase { get
        {
            GamePhase? savedPhase = SavesManager.LoadGame();
            if (savedPhase == null)
            {
                GD.Print("GameManager: No saved game found. Defaulting to Opening phase.");
                return GamePhase.Opening;
            }
            else
            {
                GD.Print($"GameManager: Loaded saved game phase: {savedPhase.Value}");
                return savedPhase.Value;
            }
        }
        private set { }
    }

    public void SetCurrentGamePhase(GamePhase phase)
    {
        CurrentGamePhase = phase;
        SavesManager.SaveGame(CurrentGamePhase);
        GD.Print($"GameManager: Current game phase set to {CurrentGamePhase}");
    }

    public void SetCurrentGameState(GameState state)
    {
        CurrentGameState = state;
        GD.Print($"GameManager: Current game state set to {CurrentGameState}");
    }

    public void ProceedPhase(SceneManager.TransitionColor color = 0, float fadeIn = 0.5f, float fadeOut = 0.5f, float sustain = 1f)
    {
        if (CurrentGamePhase == GamePhase.Cutscene_Finale)
        {
            GD.Print("GameManager: Already at final phase. Cannot proceed further.");
            SceneManager.Instance.ChangeScene(GamePhase.StartMenu, SceneManager.TransitionColor.Black, 0.5f, 0.5f, 1f);
            return;
        }

        CurrentGamePhase++;
        SavesManager.SaveGame(CurrentGamePhase);
        ChangePhase(CurrentGamePhase, color, fadeIn, fadeOut, sustain);
        GD.Print($"GameManager: Proceeded to next phase: {CurrentGamePhase}");
    }

    public void LoadPhase(SceneManager.TransitionColor color, float fadeIn = 0.5f, float fadeOut = 0.5f, float sustain = 0f)
    {
        GameManager.GamePhase? phase = SavesManager.LoadGame();
        if (phase == null)
        {
            StartNewGame();
        }
        else
        {
            CurrentGamePhase = phase.Value;
            ChangePhase(CurrentGamePhase, color, fadeIn, fadeOut, sustain);
            GD.Print($"GameManager: Loaded phase {CurrentGamePhase} from save.");
        }
    }
    
    public void ChangePhase(GamePhase phase, SceneManager.TransitionColor color, float fadeIn = 0.5f, float fadeOut = 0.5f, float sustain = 0f)
    {
        SceneManager.Instance.ChangeScene(phase, color, fadeIn, fadeOut, sustain);
    }

    public void StartNewGame()
    {
        CurrentGamePhase = GamePhase.Opening;
        SavesManager.SaveGame(CurrentGamePhase);
        ChangePhase(CurrentGamePhase, SceneManager.TransitionColor.Black, 0.5f, 0.5f, 0f);
        GD.Print("GameManager: Starting new game at Opening phase.");
    }

    public void ReturnMenu()
    {
        SceneManager.Instance.ChangeScene(GamePhase.StartMenu, SceneManager.TransitionColor.Black, 0.5f, 0.5f, 1f);
    }

    //test
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("debug_proceed"))
        {
            GD.Print("Debug: Proceeding to next phase...");
            ProceedPhase();
        }
    }
}