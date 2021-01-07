using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Handles the functionality of the game menu.
public class MenuManager : MonoBehaviour
{
    private Gamemaster gamemaster;      // The gamemaster script.
    public GameObject menuCube;         // The menu cube.
    public Transform[] spawnerPosition; // The position of the spawner.
    private float timePressed = 0.0f;   // The time for which the X-Button of the Oculus controller has been pressed.
    private int difficulty = 0;         // The selected difficulty of the game.
    private bool menuSelected = false;  // Flag which specifies whether a menu option is already selected.

    // Start is called before the first frame update
    void Start()
    {
        gamemaster = GameObject.Find("Gamemaster").GetComponent<Gamemaster>();
    }

    // Update is called once per frame
    void Update()
    {
        // If the X button of the Oculus controller is pressed for 5 seconds, quit the game (if it is running).
        if(OVRInput.Get(OVRInput.RawButton.X))
        {
            timePressed += Time.deltaTime;
        }
        if(timePressed > 5.0f)
        {
            timePressed = 0.0f;
            if(gamemaster.GameRunning) gamemaster.QuitGame();
        }
    }

    // Open the submenu that is numbered with the index.
    public void EnterMenu(int index)
    {
        menuSelected = false;   // Reset flag for choosing new menu option.
        switch(index)
        {
            // Menu with only the start button.
            case 0:
                GameObject startingCube = Instantiate(menuCube, spawnerPosition[0]);
                startingCube.GetComponentInChildren<TextMeshProUGUI>().text = "Start";
                break;
            // Menu in which the difficulty is selected.
            case 1:
                GameObject scoreBoard = GameObject.Find("ScoreBoard");
                scoreBoard.SendMessage("HideStats", SendMessageOptions.DontRequireReceiver);
                scoreBoard.SendMessage("HidePlots", SendMessageOptions.DontRequireReceiver);
                GameObject easyCube = Instantiate(menuCube, spawnerPosition[1]);
                easyCube.GetComponentInChildren<TextMeshProUGUI>().text = "Easy";
                GameObject normalCube = Instantiate(menuCube, spawnerPosition[2]);
                normalCube.GetComponentInChildren<TextMeshProUGUI>().text = "Normal";
                GameObject hardCube = Instantiate(menuCube, spawnerPosition[3]);
                hardCube.GetComponentInChildren<TextMeshProUGUI>().text = "Hard";
                break;
            // Menu in which the game can be started from the beginning or the level selection menu can be opened.
            case 2:
                GameObject startGameCube = Instantiate(menuCube, spawnerPosition[4]);
                startGameCube.GetComponentInChildren<TextMeshProUGUI>().text = "Start Game";
                GameObject chooseLevelCube = Instantiate(menuCube, spawnerPosition[5]);
                chooseLevelCube.GetComponentInChildren<TextMeshProUGUI>().text = "Choose Level";
                break;
            // Menu in which the starting level can be selected.
            case 3:
                GameObject level1Cube = Instantiate(menuCube, spawnerPosition[6]);
                GameObject level2Cube = Instantiate(menuCube, spawnerPosition[7]);
                GameObject level3Cube = Instantiate(menuCube, spawnerPosition[8]);
                GameObject level4Cube = Instantiate(menuCube, spawnerPosition[9]);
                GameObject level5Cube = Instantiate(menuCube, spawnerPosition[10]);
                GameObject level6Cube = Instantiate(menuCube, spawnerPosition[11]);
                GameObject level7Cube = Instantiate(menuCube, spawnerPosition[12]);
                GameObject level8Cube = Instantiate(menuCube, spawnerPosition[13]);

                level1Cube.GetComponentInChildren<TextMeshProUGUI>().text = "1";
                level2Cube.GetComponentInChildren<TextMeshProUGUI>().text = "2";
                level3Cube.GetComponentInChildren<TextMeshProUGUI>().text = "3";
                level4Cube.GetComponentInChildren<TextMeshProUGUI>().text = "4";
                level5Cube.GetComponentInChildren<TextMeshProUGUI>().text = "5";
                level6Cube.GetComponentInChildren<TextMeshProUGUI>().text = "6";
                level7Cube.GetComponentInChildren<TextMeshProUGUI>().text = "7";
                level8Cube.GetComponentInChildren<TextMeshProUGUI>().text = "8";
                break;
        }
    }

    // Used to open a submenu with a delay.
    IEnumerator callEnterMenu(int index)
    {
        yield return new WaitForSeconds(0.1f);
        EnterMenu(index);
    }

    // Called when a menu cube is hit. Performs the action that is bound to that cube.
    public void CubeHit(string cubeType)
    {
        if(!menuSelected) // Check if two menu options were selected at the same time.
        {
            menuSelected = true; // Set flag that a menu option was selected.
            // Remove all menu cubes that are currently shown.
            foreach(GameObject cube in GameObject.FindGameObjectsWithTag("MenuCube"))
            {
                Destroy(cube);
            }
            
            // Depending on the type of the cube, perform the corresponding action.
            switch(cubeType)
            {
                // Open the difficulty selection menu.
                case "Start":
                    StartCoroutine(callEnterMenu(1));
                    break;
                // Select easy difficulty and go to the next menu.
                case "Easy":
                    difficulty = 0;
                    StartCoroutine(callEnterMenu(2));
                    break;
                // Select normal difficulty and go to the next menu.
                case "Normal":
                    difficulty = 1;
                    StartCoroutine(callEnterMenu(2));
                    break;
                // Select hard difficulty and go to the next menu.
                case "Hard":
                    difficulty = 2;
                    StartCoroutine(callEnterMenu(2));
                    break;
                // Start the game at level 1.
                case "Start Game":
                    gamemaster.StartGame(difficulty,0);
                    break;
                // Open the level selection menu.
                case "Choose Level":
                    StartCoroutine(callEnterMenu(3));
                    break;
                // Start the game at level 1.
                case "1":
                    gamemaster.StartGame(difficulty,0);
                    break;
                // Start the game at level 2.
                case "2":
                    gamemaster.StartGame(difficulty,1);
                    break;
                // Start the game at level 3.
                case "3":
                    gamemaster.StartGame(difficulty,2);
                    break;
                // Start the game at level 4.
                case "4":
                    gamemaster.StartGame(difficulty,3);
                    break;
                // Start the game at level 5.
                case "5":
                    gamemaster.StartGame(difficulty,4);
                    break;
                // Start the game at level 6.
                case "6":
                    gamemaster.StartGame(difficulty,5);
                    break;
                // Start the game at level 7.
                case "7":
                    gamemaster.StartGame(difficulty,6);
                    break;
                // Start the game at level 8.
                case "8":
                    gamemaster.StartGame(difficulty,7);
                    break;
            }
        }
    }
}