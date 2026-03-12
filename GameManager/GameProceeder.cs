using Godot;
using System;

[GlobalClass]
public partial class GameProceeder : Node
{   
    public void Proceed(SceneManager.TransitionColor color = SceneManager.TransitionColor.Black, float fadeIn = 0.5f, float fadeOut = 0.5f, float sustain = 0f)
    {
        GD.Print("GameProceeder: Proceeding to next phase...");
        GameManager.Instance.ProceedPhase(color, fadeIn, fadeOut, sustain);
    }

    public void Restart()
    {
        GameManager.Instance.LoadPhase(SceneManager.TransitionColor.Black, 0.5f, 0.5f, 0f);
    }
}