using UnityEngine;

public class GameConfig : Singleton<GameConfig>
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
}