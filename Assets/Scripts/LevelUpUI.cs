using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// This script is responsible for displaying the level up message and effects
public class LevelUpUI : MonoBehaviour
{
    private TextMeshProUGUI levelUpMessageMesh; // Text mesh that displays the level up message.
    private ParticleSystem plasmaEffect;        // Particle system that plays the plasma explosion effect.

    private string messageText = "Level Up!";   // Message that will be displayed on a level up.
    private bool showLevelUp = false;           // Flag that indicates whether a level up message is currently being displayed.
    private float timer = 0.0f;                 // Timer used for tracking the state of the displaying process.
    private float initialDisplayTime = 1.0f;    // Time in seconds for which the message is initially displayed, before it starts blinking
    private int numberOfBlinks = 10;            // How often the message blinks
    private float timeOfBlink = 0.1f;           // How long one blink takes in seconds.
    private int displayState = -1;              // Current phase of the displaying process.
    private bool blinkOn = false;               // Flag that indicates whether the message is currently visible during the blinking phase of the displaying process.

    // Start is called before the first frame update
    void Start()
    {
        levelUpMessageMesh = this.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        plasmaEffect = GameObject.Find("PlasmaExplosionEffect").GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (showLevelUp) {
            timer += Time.deltaTime;
            // Initially display the message for a certain time.
            if (displayState < 0) {
                levelUpMessageMesh.text = messageText;
                displayState = 0;
            // After that time remove the message and initiate the blinking phase.
            } else if (displayState == 0) {
                if (timer >= initialDisplayTime) {
                    plasmaEffect.Stop();
                    timer = 0.0f;
                    displayState = 1;
                    blinkOn = false;
                    levelUpMessageMesh.text = "";
                }
            // As long as the maximum number of blinks has not been exceeded, hide or show the message, depending on the current state of it.
            } else if (displayState <= numberOfBlinks) {
                if (timer >= timeOfBlink) {
                    timer = 0.0f;
                    blinkOn = !blinkOn;
                    levelUpMessageMesh.text = blinkOn ? messageText : "";
                    if (!blinkOn) {
                        displayState += 1;
                    }                   
                }
            }
        }
    }

    // Initializes the mesh with an empty text.
    private void Initialize() {
        levelUpMessageMesh.text = "";
    }

    // Starts the displaying process of the level up message.
    public void DisplayLevelUp() {
        plasmaEffect.Play();
        this.showLevelUp = true;
        this.timer = 0.0f;
        this.displayState = -1;
    }

    // Ends the displaying process of the level up message. 
    public void StopDisplayingLevelUp() {
        plasmaEffect.Stop();
        this.showLevelUp = false;
        this.timer = 0.0f;
        levelUpMessageMesh.text = "";
    }
}
