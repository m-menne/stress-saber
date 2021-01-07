using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles the events that occur during the game.
// This includes changing the colors and rotating the room.
public class GameEventManager : MonoBehaviour
{
    // Variables needed to switch or change the colors of the game
    private Saber saberBlue;  // Reference to the right (blue) saber.
    private Saber saberRed;   // Reference to the left (red) saber.

    private Spawner spawner;  // Reference to the currently active spawner.
    public Spawner Spawner
    {
        get {return spawner;}
        set {spawner = value;}
    }
    private VisualEffects[] visualEffectManagerList = new VisualEffects[4]; // List of all visual effect manager in the game

    private Spawner[] spawnerList = new Spawner[4]; // List of all spawners in the game

    private bool switchedColors = false;    // Flag that indicates if the colors are currently reversed.
    public bool SwitchedColors 
    {
        get{return switchedColors;}
    }
    private bool colorsChanged = false;    // Flag that indicates if the colors are currently changed.
    public bool ColorsChanged 
    {
        get{return colorsChanged;}
    }
    private Material material1; // Materials set by the gamemaster
    private Material material2;

    public Material defaultMaterial1;   // Default materials of the game (Red, Blue)
    public Material defaultMaterial2;

    // Variables required to rotate the room
    private GameObject levelObject;     // Reference to the gameobject that contains the entire game scene.
    private float timeCount = 0.0f;     // Timer 
    private Vector3 initAngle;          // Initial angle of the level
    private Quaternion rotationGoal;    // Rotation Goal
    private Quaternion initialRotation; // Angle of the level before an applied rotation

    private bool rotateLevel = false;   // Flag indicating whether a rotation is in progress
    public bool RotateLevel 
    {
        get {return rotateLevel;}
    }


    // Start is called before the first frame update
    void Start()
    {
        // Set the references.
        saberBlue = GameObject.Find("LeftHandAnchor").GetComponent<Saber>();
        saberRed = GameObject.Find("RightHandAnchor").GetComponent<Saber>();
        spawner = GameObject.Find("Spawner1").GetComponent<Spawner>();
        levelObject = GameObject.Find("SceneContainer");

        for (int idx = 1; idx < 5; ++idx)
        {
            visualEffectManagerList[idx-1] = GameObject.Find("Corridor" + idx.ToString()).GetComponent<VisualEffects>();
            spawnerList[idx-1] = GameObject.Find("Spawner" + idx.ToString()).GetComponent<Spawner>();
        }

        Init();
    }

    // Update is called once per frame
    void Update()
    {    
        if(rotateLevel) ExecuteLevelRotation();
    }

    // Init method capturing the intial rotation before a level starts
    public void Init()
    {
        initAngle = levelObject.transform.rotation.eulerAngles;
    }
    
    // Switches the colors of the sabers and the cubes using the current colors in the scene
    public void SwitchColor() 
    {
        // Switch the color of the sabers
        saberBlue.SwitchColor();
        saberRed.SwitchColor();
        
        // Switch the colors in each spawner
        for (int idx = 0; idx < 4; ++idx)
        {
            spawnerList[idx].SwitchColor();
        }

        // Switch the color of all already spawned cubes
        spawner.SwitchColorAllCubes();

        switchedColors = !switchedColors;
    }

    // Changes the colors in the game to the given ones 
    public void ChangeColor(Material mat1, Material mat2) 
    {
        // If the colors are already changed, revert them to the default ones. If not, apply the new materials
        if (!colorsChanged)
        {
            material1 = mat1;
            material2 = mat2;
        }
        else 
        {
            material1 = defaultMaterial1;
            material2 = defaultMaterial2;
        }

        // At first change the color of both sabers
        saberBlue.ChangeColor(material1, material2);
        saberRed.ChangeColor(material1, material2);

        // If the colors are not reversed before changing them, apply the colors to each spawner. 
        // Otherwise switch the colors.
        if(!switchedColors)
        {
            for (int idx = 0; idx < 4; ++idx)
            {
                spawnerList[idx].ChangeColor(material1, material2);
            }
        }
        else
        {
            for (int idx = 0; idx < 4; ++idx)
            {
                spawnerList[idx].ChangeColor(material2, material1);
            }
        }

        // Change color of already spawned cubes
        spawner.ChangeColorAllCubes(material1, material2);

        // Apply the new color to all visual effect managers
        for (int idx = 0; idx < 4; ++idx)
        {
            visualEffectManagerList[idx].ChangeColors(material1, material2);
        }
            
        colorsChanged = !colorsChanged;
    }

    // Initializes the level rotation using the given values. Sets the flag to true, so that the room is rotated
    // towards the given rotation in the Update method using the ExecuteLevelRotation method. If there is a rotation
    // in progress, new angles are not applied.
    public void LevelRotation(float x_a, float y_a, float z_a)
    {
        if(!rotateLevel) {
            rotateLevel = true;

            // Initialize the roation angle
            Vector3 rotationVector = new Vector3(initAngle.x + x_a, initAngle.y + y_a, initAngle.z + z_a);
            rotationGoal = Quaternion.Euler(rotationVector);

            // Initialize the values required to rotate the room
            initialRotation = transform.rotation;
            timeCount = 0.0f;
        }
    }

    // Rotates the level / room a bit in every step, while adjusting the rotation speed to the goal angle.
    void ExecuteLevelRotation()
    {
        // Rotate a bit 
        levelObject.transform.rotation = Quaternion.Slerp(levelObject.transform.rotation, rotationGoal, timeCount*90/(1000 * Mathf.Abs((initialRotation*rotationGoal).eulerAngles.y)));
        timeCount += Time.deltaTime;

        // If the goal is reached, set then flag to false, so a new rotation can be applied
        if(levelObject.transform.rotation == rotationGoal) rotateLevel = false;
    }

    // Reset all game events if the game is quit
    public void Reset(int corridorNum)
    {
        // Rotate level back
        rotateLevel = false;
        levelObject.transform.rotation = Quaternion.Euler(new Vector3 (0.0f, 90.0f * corridorNum, 0.0f) + initAngle);        

        // Reset colors 
        if(switchedColors) SwitchColor();
        ChangeColor(defaultMaterial1, defaultMaterial2);

        // Iterate over all spawners and check if they have to be reset
        for(int idx = 0; idx < 4; ++idx)
        {
            if(switchedColors) spawnerList[idx].SwitchColor();
            if(colorsChanged) spawnerList[idx].ChangeColor(defaultMaterial1, defaultMaterial2);          
        }
    }
}
