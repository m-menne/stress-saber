using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script analyzes the audio spectrum which contains the song of the game.
// It listens for 'beats' in the song and notifies the gamemaster if one occurs.
public class AudioAnalyzer : MonoBehaviour {

	// The threshold that is used in the analysis of the song.
	// 25 seemed to work during development, although this value probably has to be adjusted if the song is changed.
    public float threshold = 25.0f;

	// The time in seconds that has to pass until the next beat is detected.
	// At 0.5 the game is playable while at the same time the cubes match the rhythm of the song used during development well enough.
	public float minTimeBetweenBeats = 0.5f;

    private GameObject gamemaster;		// Reference to the gamemaster, which is notified about a detected beat.

	private float previousAudioValue;	// The audio value that was measured last frame.
	private float currentAudioValue;	// The audio value that was measured this frame.
	private float timer;				// A timer used to track the time that passed since the last beat.

	// This flag determines whether the analyzer is active or not.
	private bool active = false;
	public bool Active {
		get {
			return active;
		}
		set {
			active = value;
		}
	}

	void Start()
    {
       gamemaster = GameObject.Find("Gamemaster");
	   timer = 0.0f;
    }

	private void Update()
	{
		if (active) {
			// Update the audio values
			previousAudioValue = currentAudioValue;
			currentAudioValue = AudioSpectrum.spectrumValue;

			// If one audio value is above and the other one is below the threshold this classifies as a beat.
			// However, this beat is only registered if the minimum time between beats has passed.
			if ((previousAudioValue <= threshold && currentAudioValue > threshold
				|| previousAudioValue > threshold && currentAudioValue <= threshold)
				&& timer >= minTimeBetweenBeats) {
				Beat();
			}

			timer += Time.deltaTime;
		}
	}

	// Notifies the gamemaster about a detected beat and resets the timer.
    public void Beat()
	{
        gamemaster.SendMessage("Beat");
		timer = 0.0f;
	}
}