using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// This class is responsible for tracking and sampling the movement of the headset and controllers.
public class PositionTracker : MonoBehaviour
{
    private float timer;                            // Timer variable.
    public float timer_limit = 0.1f;                // Defines the sampling rate in seconds.
    private float timeStamp;                        // Stores the sampling time.
    private Vector3[] dataSample = new Vector3[14]; // Variable to store the sampled data.
    private StreamWriter outstream;                 // Variable which contains the outstream writer.
    public string filePath = "TrackedMovement.csv"; // File path containing the location of the created CSV file.
    public Transform trackingSpace;                 // Contains the coordinate system reference space.
    private GameObject headset;                     // Contains the reference to the headset.
    public OVRInput.Controller controllerL;         // Contains the reference to the left controller.
    public OVRInput.Controller controllerR;         // Contains the reference to the right controller.


    // Start is called before the first frame update
    void Start()
    {
        timer = 0.0f;                                   // Initialize the timer.
        headset = GameObject.Find("CenterEyeAnchor");   // Set the reference of the headset.
        outstream = new StreamWriter(filePath);         // Create an outstream writer.
        // Write the header line for CSV file.
        outstream.WriteLine("Time;Headset Pos;;;Heatset Rot;;;LeftController Pos;;;LeftController Rot;;;LeftController Velocity;;;LeftController Acceleration;;;LeftController Angular Velocity;;;LeftController Angular Acceleration;;;RightController Pos;;;RightController Rot;;;RightController Velocity;;;RightController Acceleration;;;RightController Angular Velocity;;;RightController Angular Acceleration;;;");
    }

    // Update is called once per frame
    void Update()
    {
        // Sample and write the sampled data according to the sampling rate.
        if(timer > timer_limit)
        {    
            sampleData();
            writeDataSample();
            timer = 0.0f;
        }
        timer += Time.deltaTime;
    }

    // Sample data from headset and both touch controllers.
    void sampleData()
    {
        timeStamp = Time.time;

        // Tracking position and orientation of the headset.
        dataSample[0] = trackingSpace.TransformPoint(headset.transform.position);
        dataSample[1] = trackingSpace.TransformDirection(headset.transform.rotation.eulerAngles);

        // Tracking properties of the left touch controller movement.
        dataSample[2] = trackingSpace.TransformPoint(OVRInput.GetLocalControllerPosition(controllerL));
        dataSample[3] = trackingSpace.TransformDirection(OVRInput.GetLocalControllerRotation(controllerL).eulerAngles);
        dataSample[4] = OVRInput.GetLocalControllerVelocity(controllerL);
        dataSample[5] = OVRInput.GetLocalControllerAcceleration(controllerL);
        dataSample[6] = OVRInput.GetLocalControllerAngularVelocity(controllerL);
        dataSample[7] = OVRInput.GetLocalControllerAngularAcceleration(controllerL);

        // Tracking properties of the right touch controller movement.
        dataSample[8] = trackingSpace.TransformPoint(OVRInput.GetLocalControllerPosition(controllerR));
        dataSample[9] = trackingSpace.TransformDirection(OVRInput.GetLocalControllerRotation(controllerR).eulerAngles);
        dataSample[10] = OVRInput.GetLocalControllerVelocity(controllerR);
        dataSample[11] = OVRInput.GetLocalControllerAcceleration(controllerR);
        dataSample[12] = OVRInput.GetLocalControllerAngularVelocity(controllerR);
        dataSample[13] = OVRInput.GetLocalControllerAngularAcceleration(controllerR);
    }

    // Write sampled data to CSV file.
    void writeDataSample()
    {
        string line = timeStamp.ToString();
        
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
