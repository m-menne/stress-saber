using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using TMPro;

// This class is responsible for creating the plots.
public class Plots : MonoBehaviour
{
    private RectTransform plotContainer;                                // Reference object for all plot objects.
    List<(float, int)> heartRateValues = new List<(float, int)> {};     // List containing all measured heartrate values with assigned timestamp.

    // Start is called before the first frame update
    void Start()
    {
        plotContainer = transform.Find("PlotContainer").GetComponent<RectTransform>();
    }

    // AddHeartRateValue is called to add a new heartrate value with timestamp to the heartRateValues list.
    public void AddHeartRateValue(float time, int rate)
    {
        heartRateValues.Add((time, rate));
    }
    
    // CreateConnection creates a connecting line between the two given positions.
    private void CreateConnection(Vector2 position_A, Vector2 position_B)
    {
        // Create a new gameObject and set its parameters to form a line between the two given positions.
        GameObject gameObject = new GameObject("connection", typeof(Image));
        gameObject.transform.SetParent(plotContainer, false);
        gameObject.GetComponent<Image>().color = new Color(1,1,1, .5f);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 direction = (position_B - position_A).normalized;
        float distance = Vector2.Distance(position_A, position_B);
        rectTransform.anchorMin = new Vector2(0,0);
        rectTransform.anchorMax = new Vector2(0,0);
        rectTransform.sizeDelta = new Vector2(distance, 3f);
        rectTransform.anchoredPosition = position_A + direction * distance * .5f;
        rectTransform.localEulerAngles = new Vector3(0,0, (float)Math.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    // CreatePlot is called on the given data list to create a line plot.
    private bool CreatePlot(List<(float, int)> values_a)
    {
        ResetConnections();                     // Remove any existing connections in the plot.
        List<(float, int)> values = values_a;   // Copy the given data list.
        
        // If list is not empty, create a plot.
        if (values.Count > 0) 
        {   
            // Setting start and end time.
            float start_time = values[0].Item1;
            float end_time = values[values.Count - 1].Item1;
            float diff_time = end_time - start_time;
            // Calculate minimal and maximal y-value.
            int min_value = 1000;
            int max_value = 0;
            for(int i = 0; i < values.Count; ++i) 
            {
                if(values[i].Item2 < min_value) min_value = values[i].Item2;
                if(values[i].Item2 > max_value) max_value = values[i].Item2;
            }
            // Set minimum and maximum limit of y-axes.
            int y_axes_min = min_value - 10;
            int y_axes_max = max_value + 10;
            float diff_y_axes = y_axes_max - y_axes_min;
            // Store height and width of the plot.
            float plotHeight = plotContainer.sizeDelta.y;
            float plotWidth = plotContainer.sizeDelta.x;

            // Correct time and heartrate values.
            for(int i = 0; i < values.Count; ++i) 
            {
                values[i] = (values[i].Item1 - start_time, values[i].Item2 - y_axes_min);
            }

            // Set the labels of the plot.
            int stepSize = (int)((float)(diff_y_axes) / 5f); 
            for(int i = 1; i < 6; ++i) plotContainer.GetChild(i).GetComponent<TextMeshProUGUI>().text = (y_axes_min + (i-1) * stepSize).ToString() + " -";
            plotContainer.GetChild(6).GetComponent<TextMeshProUGUI>().text = ((int)diff_time).ToString() + "s";
            plotContainer.GetChild(7).GetComponent<TextMeshProUGUI>().text = "-";

            // Iterate over data points and set connections.
            Vector2 lastPoint = new Vector2(-1f, -1f);
            for(int i = 0; i < values.Count; ++i)
            {
                // Correct data points to fill the plot nicely.
                float x_position = (values[i].Item1 / diff_time) * plotWidth;
                float y_position = (values[i].Item2 / diff_y_axes) * plotHeight;
                
                Vector2 point = new Vector2(x_position, y_position);
                if(i != 0) // Check for creation of a new connection for every point except the first one.
                {
                    if(lastPoint[1] != point[1])    // Create a connection only if the value has changed.
                    {
                        Vector2 dummyPoint = new Vector2(point[0], lastPoint[1]);
                        CreateConnection(lastPoint,dummyPoint);
                        CreateConnection(dummyPoint, point);
                        lastPoint = point;
                    }
                }
                else
                {
                    lastPoint = point;
                }
            }
            // Create connection to the last data point.
            Vector2 endPoint = new Vector2(values[values.Count - 1].Item1 /diff_time * plotWidth, values[values.Count - 1].Item2 / diff_y_axes * plotHeight);
            CreateConnection(lastPoint, endPoint); 
        }
        // If list is empty, do not create a plot and hide the labels.
        else for(int i = 1; i < 8; ++i) plotContainer.GetChild(i).GetComponent<TextMeshProUGUI>().text = ""; // Set all labels of the plot to an empty string; hide the labels.
        
        // Flush the heartRateValues list.
        heartRateValues.Clear();
        return true;
    }

    // Creates plot with the stored heartrate values.
    public void ShowPlots()
    {
        CreatePlot(heartRateValues);
    }

    // Removes all line connections from the current plot.
    private void ResetConnections()
    {
        foreach(Transform child in plotContainer)
        {
            if(child.gameObject.name == "connection") Destroy(child.gameObject);
        }
    }
}
