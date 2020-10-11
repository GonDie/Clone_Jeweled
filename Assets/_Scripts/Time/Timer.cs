using System;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    TMP_Text _timeText;

    private void Awake()
    {
        Events.OnTimeUpdate += OnTimeUpdate;

        _timeText = GetComponent<TMP_Text>();
    }

    void OnTimeUpdate(float time)
    {
        TimeSpan ts = TimeSpan.FromSeconds(time);

        _timeText.text = $"{ts.Minutes.ToString("00")}:{ts.Seconds.ToString("00")}";
    }
}
