using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script receives the current audio spectrum data from an audio source.
public class AudioSpectrum : MonoBehaviour {
    public static float spectrumValue {get; set;}   // The value that is finally analyzed by the audio analyzer.

    private float[] audioSpectrum;                          // The spectrum in which the data is stored.

    private AudioSource rhythmSource;                       // The audio source that is used for detecting the beats.

    void Start ()
    {
        rhythmSource = this.gameObject.GetComponent<AudioSource>();
        audioSpectrum = new float[128];
    }

	void Update ()
    {
        // Receive the data from the audio source and store it in the spectrum.
        rhythmSource.GetSpectrumData(audioSpectrum, 0, FFTWindow.Hamming);
        if (audioSpectrum != null && audioSpectrum.Length > 0) {
            // If this succeeded, prepare the spectrum value for analysis by the audio analyzer.
            // The multiplier depends on the volume at which the audio source plays and the threshold that is used by the analyzer.
            spectrumValue = audioSpectrum[0] * 10000;
        }
    }
}
