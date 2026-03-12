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
        Paused
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
        Hospital_5,
        Cutscene_Hospital,
        Cutscene_Finale,
    }

    public GameState CurrentGameState { get; private set; } = GameState.Playing;
    public GamePhase CurrentGamePhase { get; private set; } = GamePhase.Opening;

    public void ProceedPhase()
    {
        if (CurrentGamePhase == GamePhase.Cutscene_Finale)
        {
            GD.Print("GameManager: Already at final phase. Cannot proceed further.");
            return;
        }

        CurrentGamePhase++;
        SavesManager.SaveGame(CurrentGamePhase);
        ChangePhase(CurrentGamePhase, new Color(0, 0, 0));
        GD.Print($"GameManager: Proceeded to next phase: {CurrentGamePhase}");
    }

    public void LoadPhase()
    {
        GameManager.GamePhase? phase = SavesManager.LoadGame();
        if (phase == null)
        {
            CurrentGamePhase = GamePhase.Opening;
            ChangePhase(CurrentGamePhase, new Color(0, 0, 0));
            GD.Print("GameManager: No saved phase found. Starting at Opening.");
        }
        else
        {
            CurrentGamePhase = phase.Value;
            ChangePhase(CurrentGamePhase, new Color(0, 0, 0));
            GD.Print($"GameManager: Loaded phase {CurrentGamePhase} from save.");
        }
    }
    
    public void ChangePhase(GamePhase phase, Color color, float fadeIn = 0.5f, float fadeOut = 0.5f, float sustain = 0f)
    {
        SceneManager.Instance.ChangeScene(phase, color, fadeIn, fadeOut, sustain);
    }
}