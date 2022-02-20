using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class UITimer : MonoBehaviour
{
    Text _timerValueText;

    int milliseconds;
    int seconds;
    int minutes;

    int secondsMultiplier = 1;

    void Start() => _timerValueText = GetComponentInChildren<Text>();

    public void SetValue(float value)
    {
        _timerValueText.text = ConvertToTimeValue(value);
    }

    public string ConvertToTimeValue(float value)
    {
        value = System.MathF.Round(value, 2);

        int Milliseconds = Mathf.RoundToInt((value - Mathf.FloorToInt(value)) * 100);
        int Minutes = Mathf.FloorToInt(value / 60);
        int Seconds = Mathf.FloorToInt(value) - Minutes * 60;

        return $"{Minutes.ToString("D2")}:{Seconds.ToString("D2")}:{Milliseconds.ToString("D2")}";
    }

    public void Reset()
    {
        milliseconds = 0;
        seconds = 0;
        minutes = 0;
        secondsMultiplier = 1;
        _timerValueText.text = "00:00:00";
    }
}
