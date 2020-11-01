using System;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    [SerializeField] float _roundTime = 120f;

    float _elapsedTime = 0f;
    bool _isPlaying;

    private void Awake()
    {
        Events.OnGameStart += OnGameStart;
        Events.OnGameEnd += OnGameEnd;
        Events.OnTimeout += OnGameEnd;
    }

    private void OnDestroy()
    {
        Events.OnGameStart -= OnGameStart;
        Events.OnGameEnd -= OnGameEnd;
        Events.OnTimeout -= OnGameEnd;
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

        if (_roundTime - _elapsedTime >= 0f)
            Events.OnTimeUpdate?.Invoke(_roundTime - _elapsedTime);
        else
            Events.OnTimeout?.Invoke();
    }
}
