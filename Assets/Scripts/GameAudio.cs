using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles all audio effects in the game.
public class GameAudio : MonoBehaviour
{
    private AudioSource musicSource;    // The audio source that plays the game music.
    private AudioSource rhythmSource;   // The audio source used to determine the rhythm.

    private float songLength;           // The length in seconds of the played song. Read-only.
    public float SongLength{get{return songLength;}}

    // The offset in seconds with which the music source starts playing after the rhythm source.
    // This is based on how long a cube needs to get from the spawner into the reach of the player.
    private float songOffset = 6.5f; 

    public AudioClip levelUpSound;          // The audio clip that is played on a level up.
    public AudioClip corridorChangedSound;  // The audio clip that is played when the currently active corridor changes.

    // Start is called before the first frame update
    void Start()
    {
        musicSource = this.gameObject.GetComponent<AudioSource>();
        rhythmSource = GameObject.Find("Rhythm").GetComponent<AudioSource>();
        rhythmSource.clip = musicSource.clip;
        songLength = musicSource.clip.length;
    }

    // Starts playing the game music.
    public void PlayMusic()
    {
        // If the audio analyzer is used for determining the beat pattern based on the song,
        // the rhythm song is started before the actual music source starts playing.
        if (rhythmSource.gameObject.GetComponent<AudioAnalyzer>().Active) {
            rhythmSource.Play();
            StartCoroutine(PlayMusicWithRhythm());
        }
        // If the analyzer is not used, only the music source is played.
        else {       
            musicSource.Play();
        }
    }

    // Waits for the time that is set by songOffset and then plays the music source.
    IEnumerator PlayMusicWithRhythm()
    {
        yield return new WaitForSeconds(songOffset);
        // The music source is only played if the rhythm source is still playing.
        // Otherwise it would start playing in the menu, if the player quit the game before the time has passed.
        if (rhythmSource.isPlaying) musicSource.Play();
    }

    // Stops playing the game music.
    public void StopMusic()
    {
        musicSource.Stop();
        rhythmSource.Stop();
    }

    // Plays the level up sound effect.
    public void PlayLevelUpEffect()
    {
        AudioSource.PlayClipAtPoint(levelUpSound, this.transform.position, 1.0f);
    }

    // Plays the corridor change sound effect from the direction of the newly activated corridor.
    // corridorIndex is the index of the new corridor (0-3).
    public void PlayCorridorChangedEffect (int corridorIndex)
    {
        // Determine the position at which the sound effect is played depending on the corridor's index.
        Vector3 position = this.transform.localPosition;
        switch (corridorIndex)
        {
            case 0:
                position.z += 4.0f;
                break;
            case 1:
                position.x += 4.0f;
                break;
            case 2:
                position.z -= 4.0f;
                break;
            case 3:
                position.x -= 4.0f;
                break;
            default:
                break;
        }
        // Play the sound effect at that position.
        AudioSource.PlayClipAtPoint(corridorChangedSound, this.transform.TransformPoint(position), 1.0f);
    }
}
