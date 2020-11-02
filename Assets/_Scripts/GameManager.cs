
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    int _currentLevel = 0;

    void Start()
    {
        PrepareGame();
    }

    public void PrepareGame()
    {
        Debug.Log("Prepare Game");
        _currentLevel++;
        Events.OnGamePrepare?.Invoke(_currentLevel);
    }

    public void StartGame()
    {
        Debug.Log("Start Game");
        Events.OnGameStart?.Invoke();
    }

    public void EndGame()
    {
        Debug.Log("End Game");
        Events.OnGameEnd?.Invoke();
    }

    public void ResetGame()
    {
        Debug.Log("Reset Game");

        _currentLevel = 0;
        PrepareGame();
    }

    public void GameTimeout()
    {
        Debug.Log("Game Timeout");
        Events.OnGameTimeout?.Invoke();
    }

    public void GameWon()
    {
        Debug.Log("Game Won");
        Events.OnGameWon?.Invoke();
    }
}