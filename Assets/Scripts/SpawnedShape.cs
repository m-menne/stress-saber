using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Moves an object that was spawned by a room spawner and destroys it eventually.
public class SpawnedShape : MonoBehaviour
{
    private float timer;
    private float movementSpeed;    // The speed with which this object moves.

    // Start is called before the first frame update
    void Start()
    {
        timer = 0.0f;
        movementSpeed = 1.0f;
        
        // Randomly initialize the objects rotation and scale.
        transform.Rotate(Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f));
        float scale = Random.Range(0.1f, 1.0f);
        transform.localScale = new Vector3(scale, scale, scale);
    }

    // Update is called once per frame
    void Update()
    {
        // Move the object upwards.
        transform.Translate(Vector3.up * Time.deltaTime*movementSpeed, Space.World);
        timer += Time.deltaTime;
        // Destroy it after 25 seconds.
        if (timer >= 25.0f) {
            Destroy(this.gameObject);
        }
    }
}
