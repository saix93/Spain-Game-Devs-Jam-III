using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/**
*   Class for the  UI timer.
*   Use to format and color.
*/
public class UI_TimeTracker : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI TimerText;
    
    [Header("Data")]
    public float MaxTime = 30f;
    public Color GoodColor = Color.white;
    public float MediumTime = 15;
    public Color MediumColor = Color.yellow;
    public float BadTime = 10;
    public Color BadColor = Color.red;

    private float timer;
    private bool working;

    private void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        if (!working) return;
        
        timer = (timer > 0) ? timer -= Time.deltaTime : 0;
        DisplayTimeFormat(timer);
        DisplayTimeColor(timer);
    }

    void DisplayTimeFormat(float timeToDisplay)
    {
        timeToDisplay = (timeToDisplay>0)? timeToDisplay + 1 : 0;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        TimerText.text = string.Format("{00}:{1:00}", minutes, seconds);
    }

    void DisplayTimeColor(float timeToDisplay)
    {
        TimerText.color = GoodColor;
        
        if (timeToDisplay < MediumTime)
            TimerText.color = MediumColor;
        if (timeToDisplay < BadTime)
            TimerText.color = BadColor;
    }

    public float GetTimeLeft()
    {
        return timer;
    }

    public void ResetTimer()
    {
        timer = MaxTime;
    }

    public void SetTimer(bool value)
    {
        working = value;
    }
}
