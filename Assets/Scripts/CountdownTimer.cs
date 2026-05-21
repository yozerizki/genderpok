using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountdownTimer : MonoBehaviour
{
    public float totalTime = 30; //Set the total time for the countdown
    public Text timerText;
    public bool timeisup;
    public bool paused = false;

    private void Start()
    {
        timeisup = false;
    }

    void Update()
    {
        if (totalTime > 0)
        {
            // Subtract elapsed time every frame
            if (!paused)
                totalTime -= Time.deltaTime;

            // Divide the time by 60
            float minutes = Mathf.FloorToInt(totalTime / 60);

            // Returns the remainder
            float seconds = Mathf.FloorToInt(totalTime % 60);

            // Set the text string
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        else
        {
            timerText.text = "Habis";
            timeisup = true;
            totalTime = 0;
        }
    }
}
