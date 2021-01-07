using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// This class displays relevant information in the UI canvases of a corridor as well as the statistics at the end of a game and the highscore list.
public class UIInformation : MonoBehaviour
{
    // Information to be displayed in the UI:
    private int accuracy;   // Displayed as (for instance) Accuracy: 95 %
    private int combo;      // Displayed as (for instance) Combo: 3x
    private int score;      // Displayed as (for instance) Score: 1000 x5 (where 5 would be the multiplier)
    private int multiplier;
    private int level;      // Displayed as (for instance) Level: 2
    private int heartrate;  // Displayed as (for instance) Heartrate: 78
    

    // References to the text meshes:
    private TextMeshProUGUI accuracyMesh;
    private TextMeshProUGUI comboMesh;
    private TextMeshProUGUI scoreMesh;
    private TextMeshProUGUI levelMesh;
    private TextMeshProUGUI heartrateMesh;

    // Start is called before the first frame update
    void Start()
    {
        // Set the references:
        Transform uiCanvasLeft = transform.GetChild(0);
        Transform uiCanvasRight = transform.GetChild(1);
        accuracyMesh = uiCanvasLeft.GetChild(0).GetComponent<TextMeshProUGUI>();
        heartrateMesh = uiCanvasLeft.GetChild(1).GetComponent<TextMeshProUGUI>();
        comboMesh = uiCanvasRight.GetChild(0).GetComponent<TextMeshProUGUI>();
        scoreMesh = uiCanvasRight.GetChild(1).GetComponent<TextMeshProUGUI>();
        levelMesh = uiCanvasRight.GetChild(2).GetComponent<TextMeshProUGUI>();

        // Initially the UI of a corridor is deactivated.
        this.Deactivate();
    }

    // Initalizes the UI with placeholder values.
    public void InitializeUI() {
        this.accuracy = 100;
        DisplayAccuracy(this.accuracy);
        this.combo = 0;
        DisplayCombo(this.combo);
        this.score = 0;
        this.multiplier = 1;
        DisplayScore(this.score, this.multiplier);
        this.level = 1;
        DisplayLevel(this.level);
        this.heartrate = 0;
        DisplayHeartrate(this.heartrate);
    }

    // Updates and displays the accuracy.
    public void DisplayAccuracy(int accuracy) {
        this.accuracy = accuracy;
        accuracyMesh.text = "Accuracy: " + accuracy.ToString() + " %";
    }

    // Updates and displays the combo.
    public void DisplayCombo(int combo) {
        this.combo = combo;
        comboMesh.text = "Combo: " + combo.ToString() + "x";
    }

    // Updates and displays the score.
    public void DisplayScore(int score, int multiplier) {
        this.score = score;
        this.multiplier = multiplier;
        scoreMesh.text = "Score: " + this.score.ToString() + " x" + this.multiplier.ToString();
    }

    // Updates and displays the level.
    // The value that is used is already the value that is finally displayed, not the internal representation starting at 0.
    public void DisplayLevel(int level) {
        this.level = level;
        levelMesh.text = "Level: " + level.ToString();
    }

    // Updates and displays the heart rate.
    public void DisplayHeartrate(int heartrate) {
        this.heartrate = heartrate;
        heartrateMesh.text = "Heartrate: " + heartrate.ToString();
    }

    // Displays the statistics at the end of the game.
    public void ShowStats(int max_combo, int score, int level, string mean_accuracy, int num_missed_Cubes, string difficulty)
    {
        Transform uiCanvasCenter = GameObject.Find("UICanvasCenter").transform;
        uiCanvasCenter.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Score: " + score.ToString();
        uiCanvasCenter.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Level: " + level.ToString();
        uiCanvasCenter.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Difficulty: " + difficulty;
        uiCanvasCenter.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Highest Combo: " + max_combo.ToString();
        uiCanvasCenter.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Mean Accuracy: " + mean_accuracy;
        uiCanvasCenter.GetChild(5).GetComponent<TextMeshProUGUI>().text = "Cubes Missed: " + num_missed_Cubes.ToString();
    }

    // Hides the statistics by setting the texts of the canvas to empty strings.
    public void HideStats()
    {
        Transform uiCanvasCenter = GameObject.Find("UICanvasCenter").transform;
        for(int i = 0; i < 6; ++i) uiCanvasCenter.GetChild(i).GetComponent<TextMeshProUGUI>().text = "";
    }

    // Displays the highscore list.
    public void ShowHighscore(List<int> scores)
    {
        Transform uiCanvasHighscore = GameObject.Find("UICanvasHighscore").transform;
        for(int i = 0; i < 5; ++i) uiCanvasHighscore.GetChild(i).GetComponent<TextMeshProUGUI>().text = (i+1).ToString() + ".\t" + scores[i].ToString();
        uiCanvasHighscore.GetChild(5).GetComponent<TextMeshProUGUI>().text = "Highscores";
    }

    // Hidey the highscore list.
    public void HideHighscore()
    {
        Transform uiCanvasHighscore = GameObject.Find("UICanvasHighscore").transform;
        for(int i = 0; i < 6; ++i) uiCanvasHighscore.GetChild(i).GetComponent<TextMeshProUGUI>().text = "";
    }

    // Empties all text meshes of the UI canvases in the corridor, making them invisible.
    public void Deactivate()
    {
        accuracyMesh.text = "";
        comboMesh.text = "";
        scoreMesh.text = "";
        levelMesh.text = "";
        heartrateMesh.text = "";
    }
}
