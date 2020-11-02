using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardTile : MonoBehaviour
{
    Transform _transform;
    public Transform Transform { get => _transform; }
    MatchPiece _matchPiece;
    public MatchPiece MatchPiece { get => _matchPiece; }
    Vector2 _boardPosition;
    public Vector2 BoardPosition { get => _boardPosition; }

    Dictionary<Direction, BoardTile> _neighbors;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _neighbors = new Dictionary<Direction, BoardTile>();
    }

    public void Init(Vector2 position, Vector2 boardPosition, BoardTile neighborUp = null, BoardTile neighborRight = null, BoardTile neighborDown = null, BoardTile neighborLeft = null)
    {
        _transform.localPosition = position;
        _boardPosition = boardPosition;

        _neighbors.Add(Direction.Up, neighborUp);
        _neighbors.Add(Direction.Right, neighborRight);
        _neighbors.Add(Direction.Down, neighborDown);
        _neighbors.Add(Direction.Left, neighborLeft);
    }

    public void SetSize(Vector2 size)
    {
        _transform.localScale = size;
    }

    public BoardTile GetNeighbor(Direction direction)
    {
        return _neighbors[direction];
    }

    public bool IsNeighbor(BoardTile tile, out Direction neighborDirection)
    {
        neighborDirection = _neighbors.FirstOrDefault(x => x.Value == tile).Key;

        return _neighbors.ContainsValue(tile);
    }

    public void SetCurrentMatchPiece(MatchPiece piece, PieceMovementType movementType)
    {
        _matchPiece = piece;

        if (piece != null)
        {
            piece.gameObject.SetActive(true);

            switch (movementType)
            {
                case PieceMovementType.Move:
                    _matchPiece.MoveToTilePosition(_transform.position);
                    break;
                case PieceMovementType.Drop:
                    _matchPiece.DropToTilePosition(_transform.position);
                    break;
            }
        }
    }
}