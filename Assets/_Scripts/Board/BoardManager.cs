using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : Singleton<BoardManager>
{
    public Transform boardContainer;
    public GameObject boardPiecePrefab;

    [Header("Board Config")]
    [SerializeField] Vector2 _boardSize = new Vector2(8, 8);
    [SerializeField] Vector2 _pieceSize = Vector2.one;
    [SerializeField] Vector2 _spacing = Vector2.zero;
    [SerializeField] float _distanceThreshold = 0.25f;

    BoardPiece[,] _board;

    Camera _camera;

    MouseState _mouseState;
    BoardPiece _selectedPiece;

    #region Tests Properties & Vars
    [Header("Tests")]
    public bool displayNeighbor;
    public GameObject displayNeighborPrefab;
    Transform[] _displayNeighbors;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        Events.OnGameStart += PrepareBoard;

        if(displayNeighbor)
            _displayNeighbors = new Transform[4] { Instantiate(displayNeighborPrefab).GetComponent<Transform>(), Instantiate(displayNeighborPrefab).GetComponent<Transform>(), Instantiate(displayNeighborPrefab).GetComponent<Transform>(), Instantiate(displayNeighborPrefab).GetComponent<Transform>() };
    }

    private void Start()
    {
        _camera = Camera.main;

        PrepareBoard();
    }

    protected override void OnDestroy()
    {
        base.Awake();

        Events.OnGameStart -= PrepareBoard;
    }

    private void Update()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hitInfo = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);

        switch (_mouseState)
        {
            case MouseState.Hovering:
                if (hitInfo.collider == null) return;

                if (Input.GetMouseButtonDown(0))
                {
                    _mouseState = MouseState.PieceSelected;
                    _selectedPiece = hitInfo.collider.GetComponent<BoardPiece>();
                }
            break;
            case MouseState.PieceSelected:
                if(Input.GetMouseButtonUp(0))
                {
                    _selectedPiece = null;
                    _mouseState = MouseState.Hovering;
                    return;
                }

                if (((Vector2)_selectedPiece.Transform.position - hitInfo.point).magnitude < _distanceThreshold)
                    return;

                float verticalDist = Mathf.Abs(_selectedPiece.Transform.position.y - hitInfo.point.y);
                float horizontalDist = Mathf.Abs(_selectedPiece.Transform.position.x - hitInfo.point.x);

                if(verticalDist > horizontalDist)
                    Debug.Log(_selectedPiece.Transform.position.y > hitInfo.point.y ? "Down" : "Up");
                else
                    Debug.Log(_selectedPiece.Transform.position.x > hitInfo.point.x ? "Left" : "Right");

                break;
        }

        #region Tests

        if (displayNeighbor && _displayNeighbors != null)
        {
            BoardPiece testNeighborPiece = hitInfo.collider.GetComponent<BoardPiece>();

            for (int i = 0; i < 4; i++)
            {
                _displayNeighbors[i].gameObject.SetActive(false);

                if (testNeighborPiece.GetNeighbor((Direction)i) != null)
                {
                    _displayNeighbors[i].gameObject.SetActive(true);
                    _displayNeighbors[i].position = testNeighborPiece.GetNeighbor((Direction)i).Transform.position;
                    _displayNeighbors[i].GetComponent<TMPro.TMP_Text>().text = ((Direction)i).ToString();
                }
            }
        }
        #endregion
    }

    void PrepareBoard()
    {
        _board = new BoardPiece[(int)_boardSize.x, (int)_boardSize.y];

        BoardPiece piece = null;
        Vector2 position = Vector2.zero;

        BoardPiece up = null;
        BoardPiece right = null;
        BoardPiece down = null;
        BoardPiece left = null;

        for (int row = 0; row < _boardSize.y; row++)
        {
            for (int col = 0; col < _boardSize.x; col++)
            {
                piece = Instantiate(boardPiecePrefab, boardContainer).GetComponent<BoardPiece>();
                _board[row, col] = piece;
            }
        }

        for (int row = 0; row < _boardSize.y; row++)
        {
            for (int col = 0; col < _boardSize.x; col++)
            {
                position.x = -((((_pieceSize.x + _spacing.x) * _boardSize.x) / 2f) - (_pieceSize.x + _spacing.x) / 2f) + (_pieceSize.x + _spacing.x) * col;
                position.y = -((((_pieceSize.y + _spacing.y) * _boardSize.y) / 2f) - (_pieceSize.y + _spacing.y) / 2f) + (_pieceSize.y + _spacing.y) * row;

                if (col == 0) left = null;
                else left = _board[row, col - 1];

                if (col < _boardSize.x - 1) right = _board[row, col + 1];
                else right = null;

                if (row == 0) down = null;
                else down = _board[row - 1, col];

                if (row < _boardSize.y - 1) up = _board[row + 1, col];
                else up = null;

                _board[row, col].Init(position, up, right, down, left);
            }
        }
    }
}