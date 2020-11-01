using UnityEngine;
using UnityEngine.UI;

public class ScoreBar : MonoBehaviour
{
    Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();

        Events.OnScoreUpdateToNextLevelPercent += OnScoreUpdate;
    }

    void OnScoreUpdate(float value)
    {
        _slider.value = value;
    }
}