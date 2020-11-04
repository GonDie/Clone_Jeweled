
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Containers")]
    public Transform PiecesContainer;
    public Transform BoardContainer;
    public Transform ParticlesContainer;
    public Transform SpawnersContainer;

    [Header("Board Config")]
    [SerializeField] Vector2 _boardSize = new Vector2(8, 8);
    public Vector2 BoardSize { get => _boardSize; }
    Vector2 _pieceSize = Vector2.one;
    public Vector2 PieceSize { get => _pieceSize; set => _pieceSize = value; }
    [SerializeField] float _tipDelay = 5f;
    public float TipDelay { get => _tipDelay; }
    [SerializeField] int _matchThreshold = 3;
    public int MatchThreshold { get => _matchThreshold; }

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
        Events.OnGameReset?.Invoke();
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