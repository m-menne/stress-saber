using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// This script detects objects that pass the miss area.
public class MissArea : MonoBehaviour
{
    private ScoreBoard scoreboard;                  // The scoreboard script.
    private float timeStamp;                        // Variable to store the current time.
    private int cubeType;                           // Variable to store the cube type (red or blue).
    private StreamWriter outstream;                 // Variable to store the outstream writer.
    private Vector3[] dataSample = new Vector3[2];  // Data sample buffer.
    public string filePath;                         // Contains the file path, where CSV files should be stored.
    public Transform room;                          // Contains the coordinate system reference room.

    // Start is called before the first frame update
    void Start ()
    {
        scoreboard = GameObject.Find("ScoreBoard").GetComponent<ScoreBoard>();  // Set reference of the scoreboard script.
        outstream = new StreamWriter(filePath);                                 // Create a new outstream writer.
        outstream.WriteLine("Time;Cube Type;Cube Pos;;;Cube Rot;;");            // Writer header line to CSV file.
    }

    // Detects and destroys objects which enter the miss area. Samples all missed cubes.
    void OnTriggerExit (Collider other) {
        if (other.gameObject.tag == "Cube" && other.gameObject.layer != 11) {       // If regular cube enters the miss area
            scoreboard.IncreaseCubesMissed();                       // Increases the missed cubes counter of the scoreboard.
            sampleData(other.gameObject);                           // Samples data of the missed cube.
            writeDataSample();                                      // Write the sampled data to CSV file.
            StartCoroutine(DestroyGameObject(other.gameObject));    // Destroys the missed cube.
        }
        else if (other.gameObject.tag == "Cube" && other.gameObject.layer == 11) {  // If do-not-hit cube enters the miss area
            StartCoroutine(DestroyGameObject(other.gameObject));    // Destroys this do-not-hit cube.
        }
        else if (other.gameObject.tag == "Frame") { // If a frame enters the miss area
            Destroy(other.gameObject);  // The frame gets destroyed.
        }
    }

    // This method destroys the given gameObject after a certain delay.
    IEnumerator DestroyGameObject(GameObject obj)
    {
        yield return new WaitForSeconds(0.01f);
        Destroy(obj);
    }

    // Samples data of missed cubes.
    void sampleData(GameObject obj)
    {
        timeStamp = Time.time;                                      // Stores the time at which the cube enters the miss area.
        cubeType = obj.layer == 9 ? 0 : 1;                          // Checks the type of cube (red or blue).
        dataSample[0] = obj.transform.localPosition;                // Stores the position of the cube.
        dataSample[1] = obj.transform.localRotation.eulerAngles;    // Stores the rotation of the cube.
    }

    // Write sampled data to CSV file.
    void writeDataSample()
    {
        string line = timeStamp.ToString();
        line += ";" + cubeType.ToString();
        
        foreach(Vector3 item in dataSample) 
        {
            for (int idx = 0; idx < 3; ++idx)
            {
                line += (";" + item[idx].ToString()); 
            }
        }
        outstream.WriteLine(line);
        outstream.Flush();
    }
}
