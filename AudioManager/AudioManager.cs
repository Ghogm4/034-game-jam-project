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
		Check();
	}

	[Export]
	public Godot.Collections.Dictionary<string, AudioStream> BGMStreams { get; set; } = new ();

	[Export] public AudioStreamPlayer BGMPlayerA;
	[Export] public AudioStreamPlayer BGMPlayerB;
	public bool CurrentPlayer = false; // false = A, true = B

	[Export] public float DefaultFadeDuration = 1.0f;
	[Export] public float DefaultMutedVolume = -80.0f;

	


	public void Check()
	{
		if (BGMPlayerA == null || BGMPlayerB == null)
		{
			GD.PrintErr("AudioManager: BGMPlayerA and BGMPlayerB must be assigned.");
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
