using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Aura2API;

// This script handles the visual effects in a corridor.
public class VisualEffects : MonoBehaviour
{
    public int corridorNumber;  // The number of the corridor (1-4).
    
    private bool beatFlipFlop;  // Flag that changes its value every beat.
    
    private ArrayList lights = new ArrayList();     // All gameobjects that are lights in the scene and supposed to be turned on every other beat.
    private ArrayList lights_1 = new ArrayList();   // All gameobjects that are lights in the scene and supposed to be turned on every other beat, asynchronously to the previous lights.

    public GameObject[] frames;						                // List of the frames that are spawned as a visual effect.
    public Material[] materials;                                    // Materials that are applied to to the lights.
    private List<GameObject> roomSpawners = new List<GameObject>(); // List of the room spawners in the scene.
    private GameObject spawner;                                     // Reference to the gameobject at which the frames shall be spawned.   
    public GameObject fogBack;                                      // Gameobject containing the fog at the back of the room.
    public GameObject fogFloor;                                     // Gameobject containing the fog at the floor of the room.

    // Flags for the individual visual effects -------------------
    private bool spawnFrames = false;
    public bool SpawnFrames
    { 
        get => spawnFrames;
        set => spawnFrames = value; 
    }

    private bool togglingLights = false;
    public bool TogglingLights
    { 
        get => togglingLights;
        set => togglingLights = value; 
    }

    private bool roomSpawnersActive = false;
    public bool RoomSpawnersActive
    { 
        get {return roomSpawnersActive;}
        set {
            roomSpawnersActive = value;
            foreach (GameObject spawner in roomSpawners) {
                spawner.SendMessage("SetState", value, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
    
    private bool fogBackActive = false;
    public bool FogBackActive
    {
        get {return fogBackActive;}
        set {
            fogBackActive = value;
            fogBack.GetComponent<AuraVolume>().enabled = value;
        }
    }
    
    private bool fogFloorActive = false;
    public bool FogFloorActive
    {
        get {return fogFloorActive;}
        set {
            fogFloorActive = value;
            fogFloor.GetComponent<AuraVolume>().enabled = value;
        }
    }
    // ---------------------------------------------------------

    // Start is called before the first frame update
    void Start () {
        beatFlipFlop = true;
        GetChildObjectWithTag(transform);
        spawner = GameObject.Find("Spawner"+corridorNumber.ToString());
        // Initially a corridor is deactivated.
        Deactivate();
    }

    // Recursively iterates through this object's children to assign the required references.
    public void GetChildObjectWithTag(Transform parent)
    {
        foreach (Transform child in parent) {
            if (child.CompareTag("Light")) {lights.Add(child.gameObject); }
            else if (child.CompareTag("Light_1")) { lights_1.Add(child.gameObject); }
            else if (child.CompareTag("Roomspawner")) { roomSpawners.Add(child.gameObject); }
            else {GetChildObjectWithTag(child);}
        }
    }

    // Called externally on every beat.
    public void OnBeat () {
        // Apply the effects that are required this beat.
        if (spawnFrames) SpawnFrame();
        if (togglingLights) ToggleLights();
        beatFlipFlop = !beatFlipFlop;   // Switch the flip flop
    }

    // Spawns a frame at the spawner. The frame's color depends on the current state of beatFlipFlop.
    void SpawnFrame () {
        if (beatFlipFlop) {
            GameObject frame = Instantiate(frames[0], spawner.transform);		// Red frame
        } else {
            GameObject frame = Instantiate(frames[1], spawner.transform);		// Blue frame
        }
    }

    // Turns all active lights off and all inactive lights on.
    void ToggleLights () {
        if (beatFlipFlop) {
            foreach (GameObject light in lights) {
                light.GetComponent<Renderer>().material = materials[11 - light.layer];   // 11 - light.layer == 2 if 'Red', 1 if 'Blue
            }
            foreach (GameObject light in lights_1) {
                light.GetComponent<Renderer>().material = materials[0];
            }
        } else {
            foreach (GameObject light in lights) {
                light.GetComponent<Renderer>().material = materials[0];
            }
            foreach (GameObject light in lights_1) {
                light.GetComponent<Renderer>().material = materials[11 - light.layer];   // 11 - light.layer == 2 if 'Red', 1 if 'Blue
            }
        }
    }
    
    // Turns all lights off.
    public void TurnAllLightsOff () {
        foreach (GameObject light in lights) {
                light.GetComponent<Renderer>().material = materials[0];
            }
        foreach (GameObject light in lights_1) {
            light.GetComponent<Renderer>().material = materials[0];
        }
        togglingLights = false;
    }

    // Turns all visual effects off. This means, that fog will disappear and room spawners will be turned off.
    // The changing lights at the floor and the walls will stop changing but remain turned on.
    public void DisableAllEffects () {
        spawnFrames = togglingLights = false;
        this.FogBackActive = false;
        this.FogFloorActive = false;
        this.RoomSpawnersActive = false;
    }

    // Turns all effects and lights in the corridor off.
    public void Deactivate () {
        DisableAllEffects();
        this.FogBackActive = true;
        TurnAllLightsOff();
        foreach(GameObject cube in GameObject.FindGameObjectsWithTag("VisualCube"))
        {
            Destroy(cube);
        }
        foreach(GameObject frame in GameObject.FindGameObjectsWithTag("Frame"))
        {
            Destroy(frame);
        }
    }

    // Brings the lights in the corridor to their initial state.
    public void Activate () {
        foreach (GameObject light in lights) {
                light.GetComponent<Renderer>().material = materials[0];
            }
        foreach (GameObject light in lights_1) {
            light.GetComponent<Renderer>().material = materials[11 - light.layer];   // 11 - light.layer == 2 if 'Red', 1 if 'Blue
        }
    }

    // Changes the materials that are used for the visual effects.
    public void ChangeColors (Material matLeft, Material matRight) {
        materials[1] = matRight;
        materials[2] = matLeft;

        // Change the materials of the prefabs that are used for the frames.
        frames[0].transform.GetComponent<Renderer>().material = matLeft;
        frames[1].transform.GetComponent<Renderer>().material = matRight;

        // Change the colors used by the room spawners.
        char[] separator = {' '};
        for (int i = 0; i < roomSpawners.Count; ++i) {
            string currentName = roomSpawners[i].name;
            if (currentName.Split(separator)[0] == "RoomSpawner_Left") {
                roomSpawners[i].GetComponent<RoomSpawner>().ChangeColor(matLeft);
            } else {
                roomSpawners[i].GetComponent<RoomSpawner>().ChangeColor(matRight);
            }

        }

        // Change the materials of the already existing frames.
        foreach(GameObject frame in GameObject.FindGameObjectsWithTag("Frame"))
        {
            if(frame.name == "Frame_Blue(Clone)")
            {
                frame.transform.GetComponent<Renderer>().material = matLeft;
            }
            else
            {
                frame.transform.GetComponent<Renderer>().material = matRight;
            }
        }

        // Change the materials of the already existing shapes that have been spawned by the room spawners.
        foreach(GameObject cube in GameObject.FindGameObjectsWithTag("VisualCube"))
        {
            if(cube.layer == 9)
            {
                cube.transform.GetComponent<Renderer>().material = matLeft;
            }
            else if (cube.layer == 10) 
            {
                cube.transform.GetComponent<Renderer>().material = matRight;
            }
        }
    }
}
