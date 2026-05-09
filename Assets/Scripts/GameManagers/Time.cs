using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
public class Time : MonoBehaviour
{
    public float timeMultiplier;
    public float initializingTime;
    public TextMeshPro timeText;
    private DateTime currentTime;
    public Light sun;
    private float sunrise;
    private float sunset;
    private TimeSpan sunriseTime;
    private TimeSpan sunsetTime;
    
    void Start()
    {
        currentTime = DateTime.Now.Date + System.TimeSpan.FromHours(initializingTime);
        sunriseTime =System.TimeSpan.FromHours(sunrise);
        sunsetTime = System.TimeSpan.FromHours(sunset);
        if (timeText == null) Debug.Log("Not Getting text");

    }
    void Update()
    {
        UpdateTime();
        RotateSun();
    }
    private TimeSpan calculateTime(TimeSpan FromTime , TimeSpan toTime)
    {
        TimeSpan difference = toTime - FromTime;
        if(difference.TotalSeconds<0)
        {
            difference += TimeSpan.FromHours(24);
        }
        return difference;
    }
    private void RotateSun()
    {
        float sunRotation;
        if (currentTime.TimeOfDay > sunriseTime && currentTime.TimeOfDay<sunsetTime)
        {
            TimeSpan sunriseToSunsetDuration = calculateTime(sunriseTime, sunsetTime);
            TimeSpan timeSinceSunrise = calculateTime(sunriseTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunrise.TotalMinutes / sunriseToSunsetDuration.TotalMinutes;
            sunRotation = Mathf.Lerp(0,180,(float)percentage);
         }
        else
        {
            TimeSpan SunsetToSunriseDuration = calculateTime(sunsetTime,sunriseTime);
            TimeSpan timeSinceSunset = calculateTime(sunsetTime, currentTime.TimeOfDay);
            double percentage = timeSinceSunset.TotalMinutes / SunsetToSunriseDuration.TotalMinutes;
            sunRotation = Mathf.Lerp(180,360,(float)percentage);
        }
        sun.transform.rotation = Quaternion.AngleAxis(sunRotation, Vector3.right);
    } 
    private void UpdateTime()
    {
        currentTime = currentTime.AddSeconds(UnityEngine.Time.deltaTime * timeMultiplier);
        if(timeText!=null)
        {
            
            timeText.text = currentTime.ToString("HH:mm");
          
        }
     //   Debug.Log(currentTime.ToString("HH:mm"));
    }
}
