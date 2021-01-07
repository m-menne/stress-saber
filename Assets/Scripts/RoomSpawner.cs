using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script that spawns shapes from a spawner in the pit of a corridor.
public class RoomSpawner : MonoBehaviour
{
    public GameObject[] spawnedShapes;  // All shapes that can be spawned by this spawner.
    public float spawnRate = 0.2f;      // The probability with which a new object is spawned every second.
    private float timer = 0.0f;         // Timer to track the time since the last object was (potentially) spawned.
    private bool active = false;        // Flag that determines whether the spawner is currently activated.

    // Update is called once per frame
    void Update()
    {
        if (active) {
            // Every second there is a chance to spawn a new object.
            if (timer >= 1.0f)
            {
                float rand = Random.Range(0.0f, 1.0f);
                if (rand < spawnRate) {
                    Spawn();
                }
                timer = 0.0f;
            }
            timer += Time.deltaTime;
        }
    }

    // Spawns an object.
    void Spawn()
    {
        // The object is randomly chosen from the list of all possible objects.
        int chosenShape = Random.Range(0, spawnedShapes.Length);
        GameObject spawnedShape = Instantiate(spawnedShapes[chosenShape], this.gameObject.transform);
    }

    // Turns the spawner on/off.
    public void SetState (bool on)
    {
        timer = 0.0f;
        active = on;
    }

    // Changes the color of all objects in the list of shapes.
    public void ChangeColor (Material newMaterial)
    {
        foreach(GameObject shape in spawnedShapes)
        {
            shape.transform.GetComponent<Renderer>().material = newMaterial;
        }
    }
}
