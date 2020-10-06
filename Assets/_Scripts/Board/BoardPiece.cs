using System.Collections.Generic;
using UnityEngine;

public class BoardPiece : MonoBehaviour
{
    Transform _transform;
    public Transform Transform { get => _transform; }

    Dictionary<Direction, BoardPiece> _neighbors;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _neighbors = new Dictionary<Direction, BoardPiece>();
    }

    public void Init(Vector2 position, BoardPiece neighborUp = null, BoardPiece neighborRight = null, BoardPiece neighborDown = null, BoardPiece neighborLeft = null)
    {
        _transform.localPosition = position;

        _neighbors.Add(Direction.Up, neighborUp);
        _neighbors.Add(Direction.Right, neighborRight);
        _neighbors.Add(Direction.Down, neighborDown);
        _neighbors.Add(Direction.Left, neighborLeft);
    }

    public BoardPiece GetNeighbor(Direction direction)
    {
        return _neighbors[direction];
    }
}