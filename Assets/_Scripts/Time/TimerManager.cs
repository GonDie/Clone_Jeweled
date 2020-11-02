using System;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    [SerializeField] float _roundTime = 120f;

    float _elapsedTime = 0f;
    bool _isPlaying;

    private void Awake()
    {
        Events.OnGamePrepare += OnGamePrepare;
        Events.OnGameStart += OnGameStart;
        Events.OnGameWon += OnGameEnd;
        Events.OnGameTimeout += OnGameEnd;
    }

    private void OnDestroy()
    {
        Events.OnGamePrepare -= OnGamePrepare;
        Events.OnGameStart -= OnGameStart;
        Events.OnGameWon -= OnGameEnd;
        Events.OnGameTimeout -= OnGameEnd;
    }

    void OnGamePrepare(int i)
    {
        _elapsedTime = 0f;
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
            GameManager.Instance.GameTimeout();
    }
}
