using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/**
*   Class for the  UI timer.
*   Use to format and color.
*/
public class UI_TimeTracker : MonoBehaviour
{
    
    private float timeValue = 45;
    public Text timerText;


    public UI_TimeTracker () {}

    public UI_TimeTracker (float newTimeValue)
    {
        timeValue = newTimeValue;
    }

    

    void Update()
    {
        timeValue = (timeValue > 0) ? timeValue -= Time.deltaTime : 0;
        DisplayTimeFormat(timeValue);
        DisplayTimeColor(timeValue);
    }

    void DisplayTimeFormat(float timeToDisplay)
    {
        timeToDisplay = (timeToDisplay>0)? timeToDisplay + 1 : 0;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timerText.text = string.Format("{00}:{1:00}", minutes, seconds);
    }

    void DisplayTimeColor(float timeToDisplay)
    {
        if (timeToDisplay >= 30)
            timerText.color = Color.white;
        if (timeToDisplay < 30)
            timerText.color = Color.yellow;
        if (timeToDisplay < 15)
            timerText.color = Color.red;
    }

    public float getTimeLeft ()
    {
        return this.timeValue;
    }

}
