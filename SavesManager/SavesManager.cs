using System;
using Godot;
using System.Text.Json;


public static class SavesManager
{
	public static void SaveGame(GameManager.GamePhase currentPhase)
	{
		var saveData = new SaveData
		{
			CurrentPhase = currentPhase
		};
		var savePath = "user://save.dat";
		using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);
		var jsonData = JsonSerializer.Serialize(saveData);
		file.StoreString(jsonData);
		GD.Print($"SavesManager: Game saved at phase {currentPhase} to {savePath}");
	}

	public static GameManager.GamePhase? LoadGame()
	{
		var savePath = "user://save.dat";
		if (!FileAccess.FileExists(savePath))
		{
			GD.Print("SavesManager: No save file found. Starting new game.");
			return null;
		}
		using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
		var jsonData = file.GetAsText();
		var saveData = JsonSerializer.Deserialize<SaveData>(jsonData);
		if (saveData != null)
		{
			GD.Print($"SavesManager: Game loaded from {savePath} at phase {saveData.CurrentPhase}");
			return saveData.CurrentPhase;
		}
		else
		{
			GD.PrintErr("SavesManager: Failed to deserialize save data. Starting new game.");
			return null;
		}
	}

	public class SaveData
	{
		public GameManager.GamePhase CurrentPhase { get; set; }
	}
}
