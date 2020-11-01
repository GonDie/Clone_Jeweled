using TMPro;
using UnityEngine;

public class TargetScore : MonoBehaviour
{
    TMP_Text _text;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();

        Events.OnTargetScoreUpdate += OnTargetScoreUpdate;
    }

    void OnTargetScoreUpdate(float value)
    {
        _text.text = value.ToString("00000000");
    }
}
