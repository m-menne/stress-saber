using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Script that handles the functionality and interaction of a saber.
public class Saber : MonoBehaviour
{
    private ScoreBoard score;   // Reference to the scoreboard to update the game information, such as score, combo ...
    
    // Variables needed to write a data sample if a cube is hit
    private float timeStamp;
    private int cubeType;
    private StreamWriter outstream;
    private Vector3[] dataSample = new Vector3[8];
    public string filePath;
    public Transform trackingSpace;
    public Transform room;
    public OVRInput.Controller controller;
    
    // Variables used by the hit detection
    private Vector3 previousPos;
    public LayerMask side;              // Used to let the correct controller vibrate
    public LayerMask currentLayer;      // Current layer of the saber
	public LayerMask layerBlue;         // Layer of both cubes that should be hit by the player
    public LayerMask layerRed;
    public LayerMask layerForbidden;    // Layer of the cube that must not be hit
    
    // Variables required to change or switch the color of a saber
    public Material saberMat1;
    public Material saberMat2;
    public GameObject saberBlade;   // Reference to the blade to change the color of the saber
    private bool colorsSwitched;


    // Start is called before the first frame update
    void Start()
    {
        // Initialize the values needed to log the hits
        cubeType = currentLayer.value == 512 ? 0 : 1; 
        score = GameObject.Find("ScoreBoard").GetComponent<ScoreBoard>();
        outstream = new StreamWriter(filePath);
        // Header line for CSV file
        outstream.WriteLine("Time;Cube Type;Cube Pos;;;Cube Rot;;;Controller Pos;;;Controller Rot;;;Controller Velocity;;;Controller Acceleration;;;Controller Angular Velocity;;;Controller Angular Acceleration;;");
    }

    // Update is called once per frame
    void Update()
    {
        // Detect hit
        RaycastHit hit;
	    if(Physics.Raycast(transform.position,transform.forward,out hit,1))
	    {
		     // Hit a normal cube
		    if((1 << hit.transform.gameObject.layer) == currentLayer.value && Vector3.Angle(transform.position-previousPos,hit.transform.up)>130)
		    {
                // Sample data
                SampleData(hit.transform.gameObject);
                // Write sampled data to CSV file
                WriteDataSample();

                // Haptic Feedback
                if(side.value == 8192) // Red Saber
                    GameObject.Find("HapticManagerLeft").GetComponent<OculusHaptics>().Vibrate(VibrationForce.Light);
                else if(side.value == 4096) // Blue Saber
                    GameObject.Find("HapticManagerRight").GetComponent<OculusHaptics>().Vibrate(VibrationForce.Light);
                
                // Set the cube to invisible
                foreach (MeshRenderer r in hit.transform.gameObject.GetComponentsInChildren(typeof(MeshRenderer)))
                {
                    r.enabled = false;
                }
                hit.transform.gameObject.GetComponent<MeshRenderer>().enabled = false;

                // Disable the boxcollider of the cube
                hit.transform.gameObject.GetComponent<BoxCollider>().enabled = false;

                // Count hits
                score.IncreaseCubesHit();

                // Activate particle system
                ParticleSystem ps = hit.transform.gameObject.GetComponent<ParticleSystem>();
                ps.Play();

                // Destroy cube
                StartCoroutine(DestroyGameObject(hit.transform.gameObject));
		    }          
            // If one hits a cube that should not be hit
            else if((1 << hit.transform.gameObject.layer) == layerForbidden.value) // && Vector3.Angle(transform.position-previousPos,hit.transform.up)>130)
            {
                // Haptic Feedback
                if(side.value == 8192) // Red Saber
                    GameObject.Find("HapticManagerLeft").GetComponent<OculusHaptics>().Vibrate(VibrationForce.Hard);
                else if(side.value == 4096) // Blue Saber
                    GameObject.Find("HapticManagerRight").GetComponent<OculusHaptics>().Vibrate(VibrationForce.Hard);

                // Set the cube to invisible
                foreach (MeshRenderer r in hit.transform.gameObject.GetComponentsInChildren(typeof(MeshRenderer)))
                {
                    r.enabled = false;
                }
                hit.transform.gameObject.GetComponent<MeshRenderer>().enabled = false;

                // Inform the scoreboard
                score.HitForbiddenCube();

                // Destroy cube
                StartCoroutine(DestroyGameObject(hit.transform.gameObject));
            }
            // If a menu cube is hit, inform the corresponding script on the cube
            else if(hit.transform.gameObject.tag == "MenuCube" && Vector3.Angle(transform.position-previousPos,hit.transform.up)>130)
            {
                hit.transform.GetComponentInChildren<MenuCube>().Hit();
            }

            previousPos = transform.position;
        }
    }

    // Wait 0.5 seconds and destroy the hit cube
    IEnumerator DestroyGameObject(GameObject obj)
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(obj);
    }

    // Samples data of cube and controllers.
    void SampleData(GameObject obj)
    {
        timeStamp = Time.time;

        // Position and rotation of the hit gameobject (Here a cube)
        dataSample[0] =  obj.transform.localPosition;
        dataSample[1] =  obj.transform.localRotation.eulerAngles;
        // Tracking properties of the touch controller movement
        dataSample[2] = trackingSpace.TransformPoint(OVRInput.GetLocalControllerPosition(controller));
        dataSample[3] = trackingSpace.TransformDirection(OVRInput.GetLocalControllerRotation(controller).eulerAngles);
        dataSample[4] = OVRInput.GetLocalControllerVelocity(controller);
        dataSample[5] = OVRInput.GetLocalControllerAcceleration(controller);
        dataSample[6] = OVRInput.GetLocalControllerAngularVelocity(controller);
        dataSample[7] = OVRInput.GetLocalControllerAngularAcceleration(controller);
    }

    // Write sampled data to CSV file.
    void WriteDataSample()
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

    // Depending on the current layer of the saber, switch the colors and layer of both saber
    public void SwitchColor()
    {
        if(currentLayer == 512) 
        {
            currentLayer = layerBlue;
            saberBlade.GetComponent<Renderer>().material = saberMat2;
        }
        else if(currentLayer == 1024)
        {
            currentLayer = layerRed;
            saberBlade.GetComponent<Renderer>().material = saberMat1;
        }
        colorsSwitched = !colorsSwitched;
    }

    // / Depending on the current layer of the saber, apply the new color to the saber and store both given colors
    public void ChangeColor(Material mat1, Material mat2) 
    {
        saberMat1 = mat1;
        saberMat2 = mat2;
        if(currentLayer == 512) 
        {
            saberBlade.GetComponent<Renderer>().material = mat1;
        }
        else if(currentLayer == 1024)
        {
            saberBlade.GetComponent<Renderer>().material = mat2;
        }
    }
}