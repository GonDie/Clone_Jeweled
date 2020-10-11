using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    float _elapsedTime = 0f;
    bool _isPlaying;

    private void Awake()
    {
        Events.OnGameStart += OnGameStart;
        Events.OnGameEnd += OnGameEnd;
    }

    private void OnDestroy()
    {
        Events.OnGameStart -= OnGameStart;
        Events.OnGameEnd -= OnGameEnd;
    }

    void OnGameStart()
    {
        _isPlaying = true;
    }

    void OnGameEnd()
    {
        _isPlaying = false;
    }

    void Update()
    {
        if (!_isPlaying)
            return;

        _elapsedTime += Time.deltaTime;

        Events.OnTimeUpdate?.Invoke(_elapsedTime);
    }
}
