using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class moves the frames that fly towards the player as part of the visual effects.
public class FrameMovement : MonoBehaviour
{
    private int rotationDirection = 1; // The direction in which the frame rotates (Clockwise or counterclockwise).

    // Start is called before the first frame update
    void Start()
    {
        // Start with a random initial rotation.
        transform.Rotate(0.0f, Random.Range(-45.0f, 45.0f), 0.0f);
        // Randomize rotation direction.
        int coinFlip = Random.Range(0,2); 
        rotationDirection = coinFlip == 1 ? 1 : -1;
    }

    // Update is called once per frame
    void Update()
    {
        // Move forward and rotate.
        transform.position += Time.deltaTime * transform.up * 20;
        transform.Rotate(0.0f, rotationDirection*5.0f, 0.0f);
    }
}
