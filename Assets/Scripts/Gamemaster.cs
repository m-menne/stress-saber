using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gamemaster : MonoBehaviour
{
	private GameObject visualManager;									// The gameobject to which the currently active visual effects are bound.
	private VisualEffects visualEffectsManager;							// The currently active script which handles the visual effects.
	private GameObject[] visualEffectManagerList = new GameObject[4];	// List of the gameobjects which contain the visual effects of the four corridors.
	public GameObject[] VisualEffectManagerList
	{
		get{return visualEffectManagerList;}
	}

	private GameObject spawner;								// The currently active spawner.
	private GameObject[] spawnerList = new GameObject[4];	// List of all four spawners in the game.
	public int corridorIndex = 0;							// The index of the current spawner.

	private GameEventManager gameEventManager;	// The script that handles the game events.
	private MenuManager menuManager;			// The script that handles the game menu.
	private GameAudio soundSystem;				// The script that handles the audio of the game.
	private LevelUpUI lvlUI;					// The script that handles the level up effects.	
	private ScoreBoard scoreBoard;				// The script that handles the calculation of various information about the current game.

	private bool gameRunning = false; // Flag that signals whether the game is currently running. Read-only.
	public bool GameRunning {get{return gameRunning;}}
	
	private float timeRunning = 0.0f;	// Time in seconds that passed since the start of the game.
	private float maxGameTime;			// Time in seconds after which the game automatically ends.
	
	// Flag that determines whether the standard beat mode is used in the game.
	// The standard mode is the mode where a cube pattern is spawned in a fixed time interval.
	// The alternative mode automatically analyzes a rhythm from the playing song and spawns the patterns according to that rhythm.
	private bool standardBeat = true;	
	public bool StandardBeat {
		get {
			return standardBeat;
		}
		set {
			standardBeat = value;
			GameObject.Find("Rhythm").GetComponent<AudioAnalyzer>().Active = !value;
		}
	}
	
	private float beatTimer;	// Timer to track the time until the next beat occurs.

	private float timeOverPercentage;	// The time in seconds for which the player managed to keep their accuracy above the required threshold for a level up.
	public float accuracyThreshold;		// The minimum accuracy that needs to be held for a certain time to reach the next level.
	private bool changeLevel;			// Flag that indicates whether the level has to be changed.
	private int level;					// The current level. Internally starting at 0.
	private int maxLevel = 8;			// The highest possible level to reach.

	// This table contains the information about which settings are applied in which level.
	private int[,] levelSettings = new int[,] { {0,0,0,0,0,0,0,0,0,0}, 	// RoomSpawner, Lights, Frames, Fog, Backfog, SwitchColors, RotateRoom, Phase, ChangeCorridor, ChangeColors
												{1,0,0,0,0,0,0,0,0,0},
												{1,1,0,0,0,0,0,1,1,0},
												{1,1,1,0,0,0,0,1,0,0},
												{1,1,1,1,0,0,0,1,1,0},
												{1,1,1,1,1,0,0,2,0,0},
												{1,1,1,1,1,1,0,2,0,0},
												{1,1,1,1,1,0,1,2,0,1},
												{1,1,1,1,1,1,1,2,1,1}
								   			  };
	
	public float timeBetweenBeats = 60.0f/115.0f;	// The time in seconds that passes between two beats.
	
    public int prob_two_cube_pattern = 40;          // Probability for generating a pattern consisting of two cubes in phases 1-3	
    public int prob_same_color = 70;                // Probability with which the cubes of a two-cube-pattern have the same color	
    public int prob_multiple_cube_pattern = 40;     // Probability for generating a pattern consisting of three cubes in phase 3
	public int prob_normal_cube = 95;				// The probability with which a normal pattern spawns instead of a cube that must not be hit.
	
	private int phase = 0;			// The current phase.
	private int beat_in_phase = 0;	// The number of the current beat in the current phase.

	// The two alternative materials that are not used at the start of the game and can be applied during the game.
	public Material mat1;
	public Material mat2;

	// References to the default materials and preshapes that are used in the game.
	public Material materialRed;
	public Material materialBlue;
	public GameObject frameBlue;
	public GameObject frameRed;
	public GameObject cubeBlue;
	public GameObject cubeRed;
	public GameObject beveledCubeBlue;
	public GameObject beveledCubeRed;

    // Start is called before the first frame update
    void Start()
    {
		// Initialize the spawner list and the currently active spawner.
		spawnerList[0] = GameObject.Find("Spawner1");
		spawnerList[1] = GameObject.Find("Spawner2");
		spawnerList[2] = GameObject.Find("Spawner3");
		spawnerList[3] = GameObject.Find("Spawner4");
		spawner = spawnerList[0];

		// Initialize the list of visual effects and the currently used script.
		visualEffectManagerList[0] = GameObject.Find("Corridor1");
		visualEffectManagerList[1] = GameObject.Find("Corridor2");
		visualEffectManagerList[2] = GameObject.Find("Corridor3");
		visualEffectManagerList[3] = GameObject.Find("Corridor4");
		visualManager = visualEffectManagerList[0];
		visualEffectsManager  = visualManager.GetComponent<VisualEffects>();

		// References to the remaining managers and scripts.
		scoreBoard = GameObject.Find("ScoreBoard").GetComponent<ScoreBoard>();		
		gameEventManager = GameObject.Find("GameEventManager").GetComponent<GameEventManager>();
		menuManager = GameObject.Find("MenuManager").GetComponent<MenuManager>();
		soundSystem = GameObject.Find("SoundSystem").GetComponent<GameAudio>();
		maxGameTime = soundSystem.SongLength;
		lvlUI = GameObject.Find("UICanvasCamera").GetComponent<LevelUpUI>();

		StandardBeat = false;	// Set the beat mode of the game.

		// Initialize spawner properties.
		InitSpawner();
		// Initialize game information.		
		beatTimer = 0.0f;
		timeOverPercentage = 0.0f;
		level = 0;
		changeLevel = false;
		InitializePrefabs();
		// Start the game in the main menu.
		menuManager.EnterMenu(0);

		scoreBoard.HidePlots();
    }

	// Initializes the current spawner.
	public void InitSpawner()
	{
		Spawner spawnerScript = spawner.GetComponent<Spawner>();
		spawnerScript.Prob_two_cube_pattern = prob_two_cube_pattern;
		spawnerScript.Prob_same_color = prob_same_color;
		spawnerScript.Prob_multiple_cube_pattern = prob_multiple_cube_pattern;
		spawnerScript.Prob_normal_cube = prob_normal_cube;
	}

    // Update is called once per frame
    void Update()
    {
		if (gameRunning) {
			// If the standard beat mode is used, the gamemaster keeps track of when to spawn a cube pattern.
			if (standardBeat) {
				if (beatTimer > timeBetweenBeats) {
					Beat();
					beatTimer = 0.0f;
				}
				beatTimer += Time.deltaTime;
			}
			// Level managment.
			if (scoreBoard.PercentHit > accuracyThreshold && !changeLevel) {
				timeOverPercentage += Time.deltaTime;
				// Check if a specific time is exceeded. If yes, apply a new level.
				if (timeOverPercentage > 20.0f) {
					if (level < maxLevel) {
						++level;
						changeLevel = true;
						timeOverPercentage = 0.0f;
					}
					// If a level up occurs while one is currently in the highest level, apply it again to trigger new game events
					else if (level == maxLevel) 
					{
						applyLevel(true);
						timeOverPercentage = 0.0f;
					}
				}
			}
			else {
				timeOverPercentage = 0.0f;
			}
			// Increase the runtime of the game and end the game if this exceeds the maximum game time.
			timeRunning += Time.deltaTime;
			if (timeRunning >= maxGameTime) 
			{
				gameRunning = false;
				StartCoroutine(QuitGameWait());
			}
		}
    }
	
	// Performs a beat.
	public void Beat()
	{   
		// Notify the visual effects manager and the spawner.
		visualManager.SendMessage("OnBeat");
		spawner.SendMessage("Beat");
		// If the level is changed, play the corresponding effects and apply the new level settings.
		if (changeLevel && gameRunning) {
			lvlUI.DisplayLevelUp();
			soundSystem.PlayLevelUpEffect();
			StartCoroutine(levelChange());
			changeLevel = false;
		}			
	}

	// This method can be used to change the level with a delay.
	IEnumerator levelChange()
    {
		scoreBoard.Level = level;
        yield return new WaitForSeconds(5.0f);
        applyLevel(true);
    }

	// Notify all necessary components of the game of the change of the level. If corridorChange is true, the corridor can be switched. 
	// This change can be omitted by setting the flag to false.
	public void applyLevel(bool corridorChange) 
	{
		// Notify the scoreboard of a level change.
		scoreBoard.Level = level;		
		// Set the phase of the spawner.
		spawner.GetComponent<Spawner>().Phase = levelSettings[level,7];
		// Set the visual effects.
		visualEffectsManager.RoomSpawnersActive = levelSettings[level, 0] == 1 ? true : false;
		visualEffectsManager.TogglingLights = levelSettings[level, 1] == 1 ? true : false;
		visualEffectsManager.SpawnFrames = levelSettings[level, 2] == 1 ? true : false;
		visualEffectsManager.FogFloorActive = levelSettings[level, 3] == 1 ? true : false;
		visualEffectsManager.FogBackActive = levelSettings[level, 4] == 1 ? true : false;

		// Depending on corridorChange apply the game effects.
		if (corridorChange) 
		{
			if (levelSettings[level, 5] == 1 && levelSettings[level, 9] == 0) gameEventManager.SwitchColor();
			else if (levelSettings[level, 5] == 0 && levelSettings[level, 9] == 1) gameEventManager.ChangeColor(mat1, mat2);
			else if (levelSettings[level, 5] == 1 && levelSettings[level, 9] == 1)
			{
				// Switch colors.
				if(Random.Range(0.0f, 1.0f) < 0.5f) gameEventManager.SwitchColor();
				// Change colors.
				else gameEventManager.ChangeColor(mat1, mat2);
			}

			// Either rotate the level or switch the corridor.
			if (levelSettings[level, 6] == 1 && levelSettings[level,8] == 0 && !gameEventManager.RotateLevel) gameEventManager.LevelRotation(0.0f,Random.Range(-180.0f, 180.0f),0.0f);
			else if (levelSettings[level, 6] == 0 && levelSettings[level,8] == 1 && !gameEventManager.RotateLevel) {
				// Choose the new corridor randomly.
				List<int> indices = new List<int>() {0,1,2,3};
				indices.RemoveAt(corridorIndex);
				changeCorridor(indices[Random.Range(0,3)]);
			}
			else if (levelSettings[level, 6] == 1 && levelSettings[level,8] == 1 && !gameEventManager.RotateLevel) 
			{
				if(Random.Range(0.0f, 1.0f) < 0.5f) gameEventManager.LevelRotation(0.0f,Random.Range(-180.0f, 180.0f),0.0f);
				else {
					// Choose the new corridor randomly.
					List<int> indices = new List<int>() {0,1,2,3};
					indices.RemoveAt(corridorIndex);
					changeCorridor(indices[Random.Range(0,3)]);
				}
			}
		}
	}

	// Changes the currently active corridor. Takes the index (0-3) of the new corridor as argument.
	public void changeCorridor(int index)
	{
		corridorIndex = index;
		// Change the active spawner.
		spawner.GetComponent<Spawner>().Phase = 0;
		spawner = spawnerList[index];
		// Deactivate the effects in the previous corridor and activate them in the new one.
		visualEffectsManager.Deactivate();
		visualManager = visualEffectManagerList[index];
		visualEffectsManager = visualManager.GetComponent<VisualEffects>();
		visualEffectsManager.Activate();
		// Notify the game event manager and the scoreboard of the change.
		gameEventManager.Spawner = spawner.GetComponent<Spawner>();
		scoreBoard.UiInfo = GameObject.Find("Corridor" + (index+1).ToString()).GetComponent<UIInformation>();
		// Initalize the spawner in the new corridor.
		InitSpawner();
		// Play a sound effect from the new corridor to notify the player of the change.
		soundSystem.PlayCorridorChangedEffect(index);
		// Reapply the level settings to keep everything in the new corridor consistent.
		applyLevel(false);
	}

	// Start the game with a given difficulty (0-2) and starting level (0-7).
	public void StartGame(int difficulty, int startLevel) {
		scoreBoard.HideInstructions();
		scoreBoard.HideHighscore();
		// Apply the starting level
		level = startLevel;
		gameEventManager.Init();
		applyLevel(true);
		// Adjust difficulty
		switch(difficulty) {
			case 0:
				scoreBoard.Difficulty = "Easy";
				prob_two_cube_pattern = 10;
				prob_same_color = 90;
				prob_multiple_cube_pattern = 30;
				prob_normal_cube = 95;
				break;

			case 1:
				scoreBoard.Difficulty = "Normal";
				prob_two_cube_pattern = 40;
				prob_same_color = 70;
				prob_multiple_cube_pattern = 40;
				prob_normal_cube = 95;
				break;

			case 2:
				scoreBoard.Difficulty = "Hard";
				prob_two_cube_pattern = 60;
				prob_same_color = 50;
				prob_multiple_cube_pattern = 45;
				prob_normal_cube = 95;
				break;

			default:
				break;
		}
		// Initialize the spawner.
		InitSpawner();
		// Start playing the music.
		soundSystem.PlayMusic();
		// Activate the visual effects and the UI information in the active corridor.
		visualEffectsManager.Activate();
		timeRunning = 0.0f;
		scoreBoard.AddingHeartRates = true;
		gameRunning = true;
	}

	// Quits the currently running game.
	public void QuitGame() {
		// Reset the current game run.
		gameRunning = false;
		soundSystem.StopMusic();
		gameEventManager.Reset(corridorIndex);
		corridorIndex = 0;
		visualEffectsManager.Deactivate();
		timeOverPercentage = 0.0f;
		level = 0;
		changeLevel = false;
		StartCoroutine(DeleteAllCubesDelay());
		spawner = spawnerList[0];
		gameEventManager.Spawner = spawner.GetComponent<Spawner>();
		visualManager = visualEffectManagerList[0];
		visualEffectsManager  = visualManager.GetComponent<VisualEffects>();
		// Make sure all prefabs have the correct materials after the game is quit.
		InitializePrefabs();
		// Enter the main menu.
		menuManager.EnterMenu(0);
		// Display the game stats, the highscore list, the instructions and plot the heart rate over the course of the game.
		scoreBoard.ShowStats();
		scoreBoard.ShowHighscore();
		scoreBoard.ShowInstructions();
		scoreBoard.ShowPlots();
		scoreBoard.AddingHeartRates = false;
		scoreBoard.Reset();
	}

	// Quits the game with a delay.
	IEnumerator QuitGameWait(float waitingTime = 5.0f)
	{
		yield return new WaitForSeconds(waitingTime);
		QuitGame();
	}

	// Destroys all spawned cubes before and after a short delay to delete all exisiting cubes in time and avoid pending cubes to spawn after the game has ended.
	IEnumerator DeleteAllCubesDelay()
	{
		spawner.GetComponent<Spawner>().DeleteAllCubes();
		yield return new WaitForSeconds(0.1f); 
		spawner.GetComponent<Spawner>().DeleteAllCubes();
	}

	// Resets the prefabs, so that they have the correct colors.
	public void InitializePrefabs()
	{
		cubeBlue.transform.GetComponent<Renderer>().material = materialBlue;
		cubeRed.transform.GetComponent<Renderer>().material = materialRed;
		frameBlue.transform.GetComponent<Renderer>().material = materialBlue;
		frameRed.transform.GetComponent<Renderer>().material = materialRed;
		beveledCubeBlue.transform.GetChild(0).gameObject.GetComponentInChildren<Renderer>().material = materialBlue;
		ParticleSystem.MainModule psMainBlue = beveledCubeBlue.GetComponent<ParticleSystem>().main;
        psMainBlue.startColor = materialBlue.color;
		beveledCubeRed.transform.GetChild(0).gameObject.GetComponentInChildren<Renderer>().material = materialRed;
		ParticleSystem.MainModule psMainRed = beveledCubeRed.GetComponent<ParticleSystem>().main;
        psMainRed.startColor = materialRed.color;
	}
}
