using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class AudioManager : Node
{
	public static AudioManager Instance { get; private set; }

	public override void _Ready()
	{
		if (Instance != null)
		{
			GD.PrintErr("Multiple instances of AudioManager detected. This should not happen.");
			QueueFree();
			return;
		}
		Instance = this;
		CheckInit();
	}

	[Export]
	public Godot.Collections.Dictionary<string, AudioStream> BGMStreams { get; set; } = new ();
	[Export]
	public Godot.Collections.Dictionary<string, AudioStream> SFXStreams { get; set; } = new ();
	[Export]
	public Godot.Collections.Dictionary<string, AudioStream> UIStreams { get; set; } = new ();

	[Export] public AudioStreamPlayer BGMPlayerA;
	[Export] public AudioStreamPlayer BGMPlayerB;

	public Godot.Collections.Array<AudioStreamPlayer> SFXPlayers = new ();
	public Godot.Collections.Array<AudioStreamPlayer> UIPlayers = new ();

	public bool CurrentPlayer = false; // false = A, true = B

	[Export] public float DefaultFadeDuration = 1.0f;
	[Export] public float DefaultMutedVolume = -80.0f;

	[Export] public float DefaultBGMVolume = -5.0f;
	[Export] public float DefaultSFXVolume = -5.0f;
	[Export] public float DefaultUIVolume = -5.0f;
	


	public void CheckInit()
	{
		if (BGMPlayerA == null || BGMPlayerB == null)
		{
			GD.PrintErr("AudioManager: BGMPlayerA and BGMPlayerB must be assigned.");
		}

		if (SFXPlayers.Count == 0)
		{
			for (int i = 0; i < 20; i ++)
			{
				AudioStreamPlayer sfxplayer = new AudioStreamPlayer();
				sfxplayer.Bus = "SFX";
				SFXPlayers.Add(sfxplayer);
				AddChild(sfxplayer);
			}
		}

		if (UIPlayers.Count == 0)
		{
			for (int i = 0; i < 10; i ++)
			{
				AudioStreamPlayer uiplayer = new AudioStreamPlayer();
				uiplayer.Bus = "UI";
				UIPlayers.Add(uiplayer);
				AddChild(uiplayer);
			}
		}
	}

	public async void PlayBGM(string name, float fadeDuration = -1)
	{
		if (fadeDuration < 0) fadeDuration = DefaultFadeDuration;

		AudioStreamPlayer currentPlayer = CurrentPlayer ? BGMPlayerB : BGMPlayerA;
		AudioStreamPlayer nextPlayer = CurrentPlayer ? BGMPlayerA : BGMPlayerB;

		if (!BGMStreams.ContainsKey(name))
		{
			GD.PrintErr($"AudioManager: BGM '{name}' not found in BGMStreams.");
			return;
		}

		GD.Print($"AudioManager: Start Playing BGM '{name}' with fade duration {fadeDuration} seconds.");
		if (currentPlayer.Stream == null)
		{
			currentPlayer.Stream = BGMStreams[name];
			currentPlayer.VolumeDb = DefaultMutedVolume;
			currentPlayer.Play();
			Tween tweenFirst = CreateTween();
			tweenFirst.TweenMethod(
				Callable.From<float>((val) => SetPlayerVolumeLinear(currentPlayer,val)),
				0.01f, 1f, fadeDuration);
			tweenFirst.TweenCallback(
				Callable.From(() => {
					GD.Print($"AudioManager: Finished fading in BGM '{name}'.");
				}));
			return;
		}
		nextPlayer.Stream = BGMStreams[name];
		nextPlayer.VolumeDb = DefaultMutedVolume;
		nextPlayer.Play();
		Tween tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenMethod(
			Callable.From<float>((val) => SetPlayerVolumeLinear(currentPlayer,val)),
			1f, 0.01f, fadeDuration);
		tween.TweenMethod(
			Callable.From<float>((val) => SetPlayerVolumeLinear(nextPlayer,val)),
			0.01f, 1f, fadeDuration);
		tween.SetParallel(false);
		tween.TweenCallback(
			Callable.From(() => {
				currentPlayer.Stop();
				CurrentPlayer = !CurrentPlayer;
				GD.Print($"AudioManager: Finished fading to BGM '{name}'.");
			}));
	}


	public void SetPlayerVolumeLinear(AudioStreamPlayer player, float volume)
	{
		player.VolumeDb = Mathf.LinearToDb(volume);
	}

	
	public void PlaySFX(string name)
	{
		if (!SFXStreams.ContainsKey(name))
		{
			GD.PrintErr($"AudioManager: SFX '{name}' not found in SFXStreams.");
			return;
		}

		foreach (AudioStreamPlayer sfxPlayer in SFXPlayers)
		{
			if (!sfxPlayer.Playing)
			{
				sfxPlayer.Stream = SFXStreams[name];
				sfxPlayer.Play();
				return;
			}
		}

		GD.PrintErr($"AudioManager: No available SFX player to play '{name}'.");
	}

	public void StopSFX(string name)
	{
		foreach (AudioStreamPlayer sfxPlayer in SFXPlayers)
		{
			if (sfxPlayer.Playing && sfxPlayer.Stream == SFXStreams[name])
			{
				sfxPlayer.Stop();
			}
		}
	}

	public void StopAllSFX()
	{
		foreach (AudioStreamPlayer sfxPlayer in SFXPlayers)
		{
			if (sfxPlayer.Playing)
			{
				sfxPlayer.Stop();
			}
		}
	}

	public void PlayUI(string name)
	{
		if (!UIStreams.ContainsKey(name))
		{
			GD.PrintErr($"AudioManager: UI sound '{name}' not found in UIStreams.");
			return;
		}

		foreach (AudioStreamPlayer uiPlayer in UIPlayers)
		{
			if (!uiPlayer.Playing)
			{
				uiPlayer.Stream = UIStreams[name];
				uiPlayer.Play();
				return;
			}
		}

		GD.PrintErr($"AudioManager: No available UI player to play '{name}'.");
	}


	// Bus control
	public void SetBusVolumeLinear(string busName, float volume)
	{
		int busIndex = AudioServer.GetBusIndex(busName);
		if (busIndex < 0)
		{
			GD.PrintErr($"AudioManager: Bus '{busName}' not found.");
			return;
		}
		AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(volume));
	}
}
