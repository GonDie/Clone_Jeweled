using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    TMP_Text _scoreText;

    private void Awake()
    {
        Events.OnScoreUpdate += OnScoreUpdate;

        _scoreText = GetComponent<TMP_Text>();
    }

    private void OnDestroy()
    {
        Events.OnScoreUpdate -= OnScoreUpdate;
    }

    void OnScoreUpdate(float value)
    {
        _scoreText.text = value.ToString("00000000");
    }
}
