using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardInput : MonoBehaviour
{
    const float DRAG_DISTANCE_THRESHOLD = 0.25f;
    const float MOUSE_CLICK_THRESHOLD = 0.15f;

    BoardManager _boardManager;

    float _mouseHoldTimer = 0f;
    MouseState _mouseState;
    BoardTile _selectedTile;

    Camera _camera;
    bool _isPlaying;

    private void Awake()
    {
        _boardManager = GetComponent<BoardManager>();

        Events.OnMouseStateEvent += OnMouseState;
        Events.OnGameStart += OnGameStart;
        Events.OnGameWon += OnGameEnd;
        Events.OnGameTimeout += OnGameEnd;
    }

    private void Start()
    {
        _camera = Camera.main;
    }

    void OnDestroy()
    {
        Events.OnMouseStateEvent -= OnMouseState;
        Events.OnGameStart -= OnGameStart;
        Events.OnGameWon -= OnGameEnd;
        Events.OnGameTimeout -= OnGameEnd;
    }

    void OnMouseState(MouseState state)
    {
        _mouseState = state;
    }

    void OnGameStart()
    {
        _isPlaying = true;
    }

    void OnGameEnd()
    {
        _isPlaying = false;
    }

    private void Update()
    {
        if (!_isPlaying) return;

        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hitInfo = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        List<BoardTile> matchesFrom = new List<BoardTile>();
        List<BoardTile> matchesTo = new List<BoardTile>();

        if (hitInfo.collider == null) return;

        switch (_mouseState)
        {
            case MouseState.Hovering:

                if (Input.GetMouseButtonDown(0))
                {
                    _mouseHoldTimer = 0f;
                    _selectedTile = hitInfo.collider.GetComponent<BoardTile>();
                    _selectedTile.MatchPiece.ToggleSelectedPiece(true);

                    Events.OnMouseStateEvent(MouseState.DragSelectedPiece);
                    SFXManager.Instance.PlaySFX(SFXType.SelectPiece);
                }

                break;
            case MouseState.DragSelectedPiece:

                _mouseHoldTimer += Time.deltaTime;

                if (Input.GetMouseButtonUp(0) && _selectedTile != null)
                {
                    if (_mouseHoldTimer <= MOUSE_CLICK_THRESHOLD)
                    {
                        Events.OnMouseStateEvent(MouseState.ClickSelectedPiece);
                        return;
                    }

                    if (((Vector2)_selectedTile.Transform.position - hitInfo.point).magnitude < DRAG_DISTANCE_THRESHOLD)
                    {
                        DeselectTile();
                        Events.OnMouseStateEvent(MouseState.Hovering);
                        return;
                    }

                    float verticalDist = Mathf.Abs(_selectedTile.Transform.position.y - hitInfo.point.y);
                    float horizontalDist = Mathf.Abs(_selectedTile.Transform.position.x - hitInfo.point.x);

                    Direction dir;
                    if (verticalDist > horizontalDist)
                        dir = _selectedTile.Transform.position.y > hitInfo.point.y ? Direction.Down : Direction.Up;
                    else
                        dir = _selectedTile.Transform.position.x > hitInfo.point.x ? Direction.Left : Direction.Right;

                    CheckTilesMatches(_selectedTile, _selectedTile.GetNeighbor(dir));
                }

                break;
            case MouseState.ClickSelectedPiece:

                if (Input.GetMouseButtonDown(0))
                {
                    _mouseHoldTimer = 0f;
                    Events.OnMouseStateEvent(MouseState.ClickSelectedOtherPiece);
                }

                break;
            case MouseState.ClickSelectedOtherPiece:

                _mouseHoldTimer += Time.deltaTime;

                if (_mouseHoldTimer > MOUSE_CLICK_THRESHOLD)
                {
                    Events.OnMouseStateEvent(MouseState.ClickSelectedPiece);
                    return;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    BoardTile otherTile = hitInfo.collider.GetComponent<BoardTile>();

                    if (_selectedTile == otherTile)
                    {
                        DeselectTile();
                        Events.OnMouseStateEvent(MouseState.Hovering);
                    }
                    else
                    {
                        Direction dir;
                        if (_selectedTile != null && _selectedTile.IsNeighbor(otherTile, out dir))
                            CheckTilesMatches(_selectedTile, otherTile);
                        else
                            Events.OnMouseStateEvent(MouseState.ClickSelectedPiece);
                    }
                }

                break;
            case MouseState.Standby:
                break;
        }
    }

    void CheckTilesMatches(BoardTile selectedTile, BoardTile otherTile)
    {
        List<BoardTile> matchesFrom = new List<BoardTile>();
        List<BoardTile> matchesTo = new List<BoardTile>();

        matchesFrom = _boardManager.CheckTileMatches(selectedTile, otherTile, otherTile.MatchPiece.pieceType);
        matchesTo = _boardManager.CheckTileMatches(otherTile, selectedTile, selectedTile.MatchPiece.pieceType);
        if (matchesFrom.Count >= GameManager.Instance.MatchThreshold || matchesTo.Count >= GameManager.Instance.MatchThreshold)
        {
            selectedTile.MatchPiece.ToggleSelectedPiece(false);
            matchesFrom.AddRange(matchesTo);
            _boardManager.ExchangeTilePieces(selectedTile, otherTile, () =>
            {
                SFXManager.Instance.PlaySFX(SFXType.MatchPiece);
                _boardManager.KillPieces(matchesFrom);
                _boardManager.TriggerScore(matchesFrom);
            });

            DeselectTile();
        }
        else
        {
            _boardManager.MoveToWrongPosition(selectedTile, otherTile, () => Events.OnMouseStateEvent(MouseState.Hovering));
            DeselectTile();
        }
    }

    void DeselectTile()
    {
        if (_selectedTile.MatchPiece != null)
            _selectedTile.MatchPiece.ToggleSelectedPiece(false);

        _selectedTile = null;
    }
}