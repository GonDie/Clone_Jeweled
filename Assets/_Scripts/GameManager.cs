
using UnityEngine;

public class GameManager : MonoBehaviour
{
    int _currentLevel = 0;

    private void Awake()
    {
        Events.CallPrepareGame += PrepareGame;
        Events.CallStartGame += StartGame;
        Events.CallEndGame += EndGame;
        Events.CallResetGame += ResetGame;
        Events.CallGameTimeout += GameTimeout;
        Events.CallGameWon += GameWon;
    }

    private void OnDestroy()
    {
        Events.CallPrepareGame -= PrepareGame;
        Events.CallStartGame -= StartGame;
        Events.CallEndGame -= EndGame;
        Events.CallResetGame -= ResetGame;
        Events.CallGameTimeout -= GameTimeout;
        Events.CallGameWon -= GameWon;
    }

    void Start()
    {
        PrepareGame();
    }

    void PrepareGame()
    {
        Debug.Log("Prepare Game");
        _currentLevel++;
        Events.OnGamePrepare?.Invoke(_currentLevel);
    }

    void StartGame()
    {
        Debug.Log("Start Game");
        Events.OnGameStart?.Invoke();
    }

    void EndGame()
    {
        Debug.Log("End Game");
        Events.OnGameEnd?.Invoke();
    }

    void ResetGame()
    {
        Debug.Log("Reset Game");

        _currentLevel = 0;
        Events.OnGameReset?.Invoke();
        PrepareGame();
    }

    void GameTimeout()
    {
        Debug.Log("Game Timeout");
        Events.OnGameTimeout?.Invoke();
    }

    void GameWon()
    {
        Debug.Log("Game Won");
        Events.OnGameWon?.Invoke();
    }
}