using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

// Handles the calculation and management of various in-game information.
public class ScoreBoard : MonoBehaviour
{
    private bool addingHeartRates = false;
    public bool AddingHeartRates{get{return addingHeartRates;} set{addingHeartRates = value;}}
    private int cubesTotal;         // The total number of cubes that appeared in the game this far.
    private int cubesHit;           // The number of allowed cubes that have been hit.    
    private int forbiddenCubesHit;  // The number of forbidden cubes that have been hit.
    private int cubesMissed;        // The number of cubes that have been missed.

    private int combo;              // The current combo.
    private int max_combo = 0;      // Stores the highest reached combo in the current game.
    private int score = 0;
    private int multiplier = 1;

    private string difficulty = "";
    public string Difficulty {set {difficulty = value;}}

    private float percentHit;   // How many percent of all cubes have been hit.
    public float PercentHit 
    {
        get {return percentHit;}
        set {percentHit = value;} 
    }

    private int heart_rate = 0; // The last received value for the heart rate.
    public int Heart_rate 
    {
        get {return heart_rate;}
        set {
            // Write the received heart rate to the CSV files and the UI information immediately.
            heart_rate = value;
            writeHeartRate(heart_rate);
            uiInfo.DisplayHeartrate(heart_rate);
        } 
    }

    private float time_m;   // The current time.

    public int numberOfLoggedValues;
    public List<int> loggedValues = new List<int>();    // List containing the logged hits and misses

    private StreamWriter outstream;     // Reference to the outstream
	public string filePath;         

    private UIInformation uiInfo;   // Reference to the currently used UI information script.
    public UIInformation UiInfo {
        get {return uiInfo;}
        set {
            // If the script changes, deactivate the old UI information and activate the new one.
            uiInfo.Deactivate();
            uiInfo = value;
            uiInfo.InitializeUI();
            uiInfo.DisplayLevel(level+1);
        }
    }

    private GameObject instructions;    // Reference to the instructions

    private Plots plots;    // Reference to the plots

    private int level = 0;  // The current level.
    public int Level {
        get {return level;}
        set {
            // If the level changes, immediately display it in the UI information.
            level = value;
            uiInfo.DisplayLevel(level+1);
        }
    }    

    // Called before all Start methods are executed
    void Awake()
    {
        // Check if the directory exists. If not, create the directory.
        Directory.CreateDirectory(@Directory.GetCurrentDirectory() + "\\DataLog");

        // Check if the highscores.txt file exists. If not, create it. 
        if(!File.Exists(@Directory.GetCurrentDirectory() + "\\DataLog\\highscores.txt")) 
        {
            string[] initScoreValues = {"949400","482400","326500","255000","1100"};
            System.IO.File.WriteAllLines(@Directory.GetCurrentDirectory() + "\\DataLog\\highscores.txt", initScoreValues);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        // Initialize the information.
        cubesTotal = cubesHit = cubesMissed = combo = heart_rate = max_combo = forbiddenCubesHit =  0;
        percentHit = 1.0f;
        uiInfo = GameObject.Find("Corridor1").GetComponent<UIInformation>();
        outstream = new StreamWriter(filePath);
        // Header line for CSV file
        outstream.WriteLine("Time;Heart Rate");
        outstream.Flush();
 
        // Find the instructions and plot gameobject
        instructions = GameObject.Find("Instructions");
        plots = GameObject.Find("Plots").GetComponent<Plots>();

        // Display the highscores
        ShowHighscore();
    }

    // Update is called once per frame
    void Update()
    {
        time_m = Time.time;
    }

    // Increases the number of hit cubes and all corresponding information.
    public void IncreaseCubesHit () {
        ++cubesHit;
        ++cubesTotal;
        ++combo;
        // Calculate the current multiplier, which is at least 1 and capped to 10
        multiplier = combo < 100 ? multiplier = ((combo - (combo % 10)) / 10) + 1 : multiplier = 10;
        // Update the score, display it and update the accuracy
        score += (100 * (level+1)) * multiplier;
        uiInfo.DisplayScore(score, multiplier);
        UpdatePercentHit(true);
    }

    // Increases the number of missed cubes and all corresponding information.
    public void IncreaseCubesMissed () {
        ++cubesMissed;
        if(combo > max_combo) max_combo = combo;
        combo = 0;
        ++cubesTotal;
        UpdatePercentHit(false);
    }

    // Increases the number of forbidden cubes that have been hit and resets the combo.
    public void HitForbiddenCube()
    {
        ++forbiddenCubesHit;
        if(combo > max_combo) max_combo = combo;
        combo = 0;
    }

    // Updates the accuracy.
    private void UpdatePercentHit (bool cubeHit)
    {
        // Log the value and update the accuracy
        if (loggedValues.Count < numberOfLoggedValues)
        {
            if (cubeHit) loggedValues.Insert(0, 1);
            else loggedValues.Insert(0, 0);
            percentHit = calculatePercantageHit();
        }
        // If the number of entries in the list equals the upper limit, additionally delete the oldest element
        else 
        {
            if (cubeHit) loggedValues.Insert(0, 1);
            else loggedValues.Insert(0, 0);
            loggedValues.RemoveAt(loggedValues.Count - 1);
            percentHit = calculatePercantageHit();
        }

        // Display the new accuracy value and the new combo
        uiInfo.DisplayAccuracy((int)(percentHit*100.0f));
        uiInfo.DisplayCombo(combo);
    }

    // Calculates the percantage of hit cubes in the list containing all destroyed cubes
    private float calculatePercantageHit()
    {
        int hitCubes = 0;

        // Iterate over the list and count the hit cubes
        for (int idx = 0; idx < loggedValues.Count; ++idx) 
        {
            if (loggedValues[idx] == 1)
            {
                ++hitCubes;
            } 
        }
        
        // Divide the number of hit cubes by total number of entries in the list
        return (float)hitCubes / (float)loggedValues.Count;
    }

    // Writes the heart rate into the CSV files.
    private void writeHeartRate(int heart_rate_a)
    {
        string line = time_m.ToString() + ";" + heart_rate_a.ToString();

        outstream.WriteLine(line);
        outstream.Flush();

        if(heart_rate_a > 10 && addingHeartRates) plots.AddHeartRateValue(time_m, heart_rate_a);
    }

    // Displays the statistics
    public void ShowStats()
    {
        if(combo > max_combo) max_combo =  combo;
        uiInfo.ShowStats(max_combo, score, level+1, cubesTotal == 0 ? "100 %" : ((int)(((float)cubesHit/(float)cubesTotal)*100)).ToString() + " %", cubesMissed + forbiddenCubesHit, difficulty);
    }

    // Hides the statistics
    public void HideStats()
    {
        uiInfo.HideStats();
    }

    // Displays the highscore list
    public void ShowHighscore()
    {
        List<int> scores = new List<int>();
        string scoreLine = System.IO.File.ReadAllText(@Directory.GetCurrentDirectory() + "\\DataLog\\highscores.txt");
        string[] scoreValues = scoreLine.Split('\n');
        for(int i = 0; i < 5; ++i) scores.Add(Int32.Parse(scoreValues[i]));
        scores.Add(score);
        scores.Sort();
        scores.Reverse();
        scores.RemoveAt(5);
        uiInfo.ShowHighscore(scores);
        for(int i = 0; i < 5; ++i) scoreValues[i] = scores[i].ToString();
        System.IO.File.WriteAllLines(@Directory.GetCurrentDirectory() + "\\DataLog\\highscores.txt", scoreValues);
        score = 0;
    }

    // Hides the highscore list
    public void HideHighscore()
    {
        uiInfo.HideHighscore();
    }

    // Shows the instructions 
    public void ShowInstructions()
    {
        instructions.SetActive(true);
    }

    // Hides the instructions
    public void HideInstructions()
    {
        instructions.SetActive(false);
    }

    // Shows the plots
    public void ShowPlots()
    {
        plots.gameObject.SetActive(true);
        plots.ShowPlots();
    }

    // Hides the plots
    public void HidePlots()
    {
        plots.gameObject.SetActive(false);
    }


    // Resets the scoreboard.
    public void Reset()
    {
        loggedValues.Clear();
        cubesTotal = cubesHit = cubesMissed = combo = heart_rate = max_combo = forbiddenCubesHit =  0;
        percentHit = 1.0f;
        uiInfo.Deactivate();
        uiInfo = GameObject.Find("Corridor1").GetComponent<UIInformation>();
    }
}
