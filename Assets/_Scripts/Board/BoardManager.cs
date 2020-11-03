using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BoardManager : Singleton<BoardManager>
{
    const int PIECE_MATCH_THRESHOLD = 3;
    const float DRAG_DISTANCE_THRESHOLD = 0.25f;
    const float MOUSE_CLICK_THRESHOLD = 0.15f;
    const float PIECE_SCALE_MULTIPLIER = 100f;

    public Transform piecesContainer;
    public Transform boardContainer;
    public Transform particlesContainer;
    public Transform spawnersContainer;

    [SerializeField, SerializeReference]
    public ScriptableObject[] patterns;

    [Header("Board Config")]
    [SerializeField] Vector2 _boardSize = new Vector2(8, 8);
    public Vector2 BoardSize { get => _boardSize; }
    Vector2 _pieceSize = Vector2.one;
    public Vector2 PieceSize { get => _pieceSize; }
    [SerializeField] float tipDelay = 5f;
    public int MatchThreshold { get => PIECE_MATCH_THRESHOLD; }

    BoardTile[,] _board;
    Transform[] _spawnersTransform;

    Camera _camera;

    MouseState _mouseState;
    BoardTile _selectedTile;

    bool _isPlaying;
    float _mouseHoldTimer = 0f;
    float _tipTimer = 0f;
    BoardTile _tileTip;

    protected override void Awake()
    {
        base.Awake();

        Events.OnGameStart += OnGameStart;
        Events.OnGameWon += OnGameEnd;
        Events.OnGameTimeout += OnGameEnd;
    }

    private void Start()
    {
        _camera = Camera.main;

        StartCoroutine(PrepareBoard());
    }

    protected override void OnDestroy()
    {
        base.Awake();

        Events.OnGameStart -= OnGameStart;
        Events.OnGameWon -= OnGameEnd;
        Events.OnGameTimeout -= OnGameEnd;
    }

    private void Update()
    {
        if (!_isPlaying) return;

        _tipTimer += Time.deltaTime;
        if (_tipTimer >= tipDelay && _tileTip != null && _mouseState == MouseState.Hovering)
        {
            _tipTimer = 0f;
            _tileTip.MatchPiece.ToggleTip(true);
        }

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
                    _mouseState = MouseState.DragSelectedPiece;
                    _selectedTile = hitInfo.collider.GetComponent<BoardTile>();
                    _selectedTile.MatchPiece.ToggleSelectedPiece(true);

                    SFXManager.Instance.PlaySFX(SFXType.SelectPiece);
                }

            break;
            case MouseState.DragSelectedPiece:

                _mouseHoldTimer += Time.deltaTime;

                if (Input.GetMouseButtonUp(0))
                {
                    if (_mouseHoldTimer <= MOUSE_CLICK_THRESHOLD)
                    {
                        _mouseState = MouseState.ClickSelectedPiece;
                        return;
                    }

                    if (((Vector2)_selectedTile.Transform.position - hitInfo.point).magnitude < DRAG_DISTANCE_THRESHOLD)
                    {
                        DeselectTile();
                        _mouseState = MouseState.Hovering;
                        return;
                    }

                    float verticalDist = Mathf.Abs(_selectedTile.Transform.position.y - hitInfo.point.y);
                    float horizontalDist = Mathf.Abs(_selectedTile.Transform.position.x - hitInfo.point.x);

                    Direction dir;
                    if (verticalDist > horizontalDist)
                        dir = _selectedTile.Transform.position.y > hitInfo.point.y ? Direction.Down : Direction.Up;
                    else
                        dir = _selectedTile.Transform.position.x > hitInfo.point.x ? Direction.Left : Direction.Right;

                    matchesFrom = CheckTileMatches(_selectedTile, _selectedTile.GetNeighbor(dir), _selectedTile.GetNeighbor(dir).MatchPiece.pieceType);
                    matchesTo = CheckTileMatches(_selectedTile.GetNeighbor(dir), _selectedTile, _selectedTile.MatchPiece.pieceType);
                    if (matchesFrom.Count >= PIECE_MATCH_THRESHOLD || matchesTo.Count >= PIECE_MATCH_THRESHOLD)
                    {
                        _selectedTile.MatchPiece.ToggleSelectedPiece(false);
                        matchesFrom.AddRange(matchesTo);
                        ExchangeTilePieces(_selectedTile, _selectedTile.GetNeighbor(dir), () =>
                        {
                            SFXManager.Instance.PlaySFX(SFXType.MatchPiece);
                            KillPieces(matchesFrom, TriggerScore);
                        });

                        DeselectTile();
                    }
                    else
                    {
                        MoveToWrongPosition(_selectedTile, _selectedTile.GetNeighbor(dir), () => _mouseState = MouseState.Hovering);
                        DeselectTile();
                    }
                }

            break;
            case MouseState.ClickSelectedPiece:

                if (Input.GetMouseButtonDown(0))
                {
                    _mouseHoldTimer = 0f;
                    _mouseState = MouseState.ClickSelectedOtherPiece;
                }

            break;
            case MouseState.ClickSelectedOtherPiece:

                _mouseHoldTimer += Time.deltaTime;

                if (_mouseHoldTimer > MOUSE_CLICK_THRESHOLD)
                {
                    _mouseState = MouseState.ClickSelectedPiece;
                    return;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    BoardTile otherTile = hitInfo.collider.GetComponent<BoardTile>();

                    if (_selectedTile == otherTile)
                    {
                        DeselectTile();
                        _mouseState = MouseState.Hovering;
                    }
                    else
                    {
                        Direction dir;
                        if (_selectedTile != null && _selectedTile.IsNeighbor(otherTile, out dir))
                        {
                            matchesFrom = CheckTileMatches(_selectedTile, otherTile, otherTile.MatchPiece.pieceType);
                            matchesTo = CheckTileMatches(otherTile, _selectedTile, _selectedTile.MatchPiece.pieceType);
                            if (matchesFrom.Count >= PIECE_MATCH_THRESHOLD || matchesTo.Count >= PIECE_MATCH_THRESHOLD)
                            {
                                _selectedTile.MatchPiece.ToggleSelectedPiece(false);
                                matchesFrom.AddRange(matchesTo);
                                ExchangeTilePieces(_selectedTile, otherTile, () =>
                                {
                                    SFXManager.Instance.PlaySFX(SFXType.MatchPiece);
                                    KillPieces(matchesFrom, TriggerScore);
                                });

                                DeselectTile();
                            }
                            else
                            {
                                MoveToWrongPosition(_selectedTile, otherTile, () => _mouseState = MouseState.Hovering);
                                DeselectTile();
                            }
                        }
                        else
                        {
                            _mouseState = MouseState.ClickSelectedPiece;
                        }
                    }
                }

            break;
            case MouseState.Standby:
            break;
        }
    }

    void OnGameStart()
    {
        _isPlaying = true;

        FillBoard();
    }

    void OnGameEnd()
    {
        _isPlaying = false;

        ClearBoard((float f, Vector3 v3) => StopAllCoroutines());
    }

    void DeselectTile()
    {
        if(_selectedTile.MatchPiece != null)
            _selectedTile.MatchPiece.ToggleSelectedPiece(false);
        
        _selectedTile = null;
    }

    IEnumerator PrepareBoard()
    {
        Camera cam = Camera.main;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float boardBoundariesW = screenWidth * 0.9f;
        float boardBoundariesH = screenHeight * 0.8f;

        float scaleH = boardBoundariesH / boardBoundariesW;
        float scaleV = boardBoundariesW / boardBoundariesH;

        float pieceScale = (scaleH < scaleV ? boardBoundariesH / _boardSize.y : boardBoundariesW / _boardSize.x);
        Vector3 position = Vector3.zero;

        _pieceSize = Vector2.one * pieceScale;
        _board = new BoardTile[(int)_boardSize.x, (int)_boardSize.y];

        BoardTile tile = null;

        BoardTile up = null;
        BoardTile right = null;
        BoardTile down = null;
        BoardTile left = null;

        for (int row = 0; row < _boardSize.y; row++)
        {
            for (int col = 0; col < _boardSize.x; col++)
            {
                tile = null;
                Addressables.InstantiateAsync("BoardTile", boardContainer).Completed += handler =>
                {
                    tile = handler.Result.GetComponent<BoardTile>();
                };

                yield return new WaitUntil(() => tile != null);
                _board[row, col] = tile;
            }
        }

        for (int row = 0; row < _boardSize.y; row++)
        {
            for (int col = 0; col < _boardSize.x; col++)
            {
                position.x = (screenWidth / 2f) - (((_pieceSize.x * _boardSize.x) / 2f) - _pieceSize.x / 2f) + _pieceSize.x * col;
                position.y = (screenHeight / 2f) - (((_pieceSize.y * _boardSize.y) / 2f) - _pieceSize.y / 2f) + _pieceSize.y * row;
                position.z = 0f;

                if (col == 0) left = null;
                else left = _board[row, col - 1];

                if (col < _boardSize.x - 1) right = _board[row, col + 1];
                else right = null;

                if (row == 0) down = null;
                else down = _board[row - 1, col];

                if (row < _boardSize.y - 1) up = _board[row + 1, col];
                else up = null;

                _board[row, col].Init(boardContainer.position + cam.ScreenToWorldPoint(position), new Vector2(row, col), up, right, down, left);
            }

            for (int col = 0; col < _boardSize.x; col++)
            {
                _board[row, col].SetSize(Vector2.one * Mathf.Abs(_board[0, 0].Transform.localPosition.x - _board[0, 1].Transform.localPosition.x));
            }
        }

        _pieceSize = Vector2.one * Mathf.Abs(_board[0, 0].Transform.localPosition.x - _board[0, 1].Transform.localPosition.x);

        Transform spawnTrans;
        Vector3 spawnPosition = cam.ScreenToWorldPoint(Vector3.up * (screenHeight + 100f));
        _spawnersTransform = new Transform[(int)_boardSize.x];

        for (int col = 0; col < _boardSize.x; col++)
        {
            spawnTrans = null;
            Addressables.InstantiateAsync("Spawner", spawnersContainer).Completed += handler =>
            {
                spawnTrans = handler.Result.GetComponent<Transform>();
            };

            yield return new WaitUntil(() => spawnTrans != null);

            spawnTrans.position = boardContainer.position + new Vector3(_board[0, col].Transform.localPosition.x, spawnPosition.y, 0f);
            _spawnersTransform[col] = spawnTrans;
        }
    }

    void FillBoard()
    {
        _mouseState = MouseState.Standby;

        for (int col = 0; col < (int)_boardSize.x; col++)
        {
            StartCoroutine(FillColumn(col));
        }

        StartCoroutine(WaitForBoardReady(() =>
        {
            CheckBoardMatches();
        }));
    }

    IEnumerator FillColumn(int columnIndex)
    {
        MatchPiece piece = null;
        BoardTile tile = null;
        for(int row = 0; row < (int)_boardSize.y; row++)
        {
            piece = null;
            tile = _board[row, columnIndex];
            if (tile.MatchPiece != null)
                continue;

            while (true)
            {
                tile = tile.GetNeighbor(Direction.Up);
                if (tile == null)
                    break;

                if (tile.MatchPiece != null)
                {
                    piece = tile.MatchPiece;
                    tile.SetCurrentMatchPiece(null, PieceMovementType.Drop);
                    break;
                }
            }

            if (piece == null)
            {
                ObjectPooler.Instance.GetObject(((PieceType)Random.Range(0, (int)PieceType.Count)).ToString(), (MatchPiece pooledPiece) =>
                {
                    piece = pooledPiece;
                    piece.Transform.SetParent(piecesContainer);
                    piece.Transform.position = _spawnersTransform[columnIndex].position;
                    piece.Transform.localScale = _pieceSize;
                    _board[row, columnIndex].SetCurrentMatchPiece(piece, PieceMovementType.Drop);
                });

                yield return new WaitUntil(() => piece != null);
            }
            else
                _board[row, columnIndex].SetCurrentMatchPiece(piece, PieceMovementType.Drop);
        }
    }

    void CheckBoardMatches()
    {
        List<BoardTile> matches = new List<BoardTile>();
        List<BoardTile> tiles;
        for (int row = 0; row < (int)_boardSize.x; row++)
        {
            for (int col = 0; col < (int)_boardSize.x; col++)
            {
                tiles = CheckTileMatches(_board[row, col], null, _board[row, col].MatchPiece.pieceType);
                if (tiles.Count >= PIECE_MATCH_THRESHOLD)
                {
                    matches.AddRange(tiles.Where(x => !matches.Contains(x)));
                }
            }
        }

        if (matches.Count >= PIECE_MATCH_THRESHOLD)
        {
            SFXManager.Instance.PlaySFX(SFXType.MatchPiece);
            KillPieces(matches, TriggerScore);
        }
        else
        {
            _tileTip = null;
            _tileTip = GetFirstPossibleBoardMatch();
            if (_tileTip != null)
                _mouseState = MouseState.Hovering;
            else
                ClearBoard((float f, Vector3 v3) => FillBoard());
        }
    }

    List<BoardTile> CheckTileMatches(BoardTile tile, BoardTile ignoreOther, PieceType type)
    {
        List<BoardTile> tempMatches = new List<BoardTile>();
        List<BoardTile> tilesMatched = new List<BoardTile>();

        for(int i = 0; i < patterns.Length; i++)
        {
            if (!(patterns[i] is IMatchPattern))
                continue;

            tempMatches.Clear();
            tempMatches = ((IMatchPattern)patterns[i]).CheckMatches(tile, ignoreOther, type);

            tilesMatched.AddRange(tempMatches.Where(x => !tilesMatched.Contains(x)));
        }

        return tilesMatched;
    }

    BoardTile GetFirstPossibleBoardMatch()
    {
        BoardTile tile = null;
        for (int row = 0; row < _boardSize.y; row++)
        {
            for (int col = 0; col < _boardSize.x; col++)
            {
                for (int i = 0; i < (int)Direction.Count; i++)
                {
                    if (_board[row, col].GetNeighbor((Direction)i) != null && 
                        CheckTileMatches(_board[row, col].GetNeighbor((Direction)i), _board[row, col], _board[row, col].MatchPiece.pieceType).Count >= PIECE_MATCH_THRESHOLD)
                    {
                        tile = _board[row, col];
                        return tile;
                    }
                }
            }
        }

        return tile;
    }

    void ExchangeTilePieces(BoardTile fromTile, BoardTile toTile, SimpleEvent callback = null)
    {
        SFXManager.Instance.PlaySFX(SFXType.SwapPiece);

        _mouseState = MouseState.Standby;

        MatchPiece piece = fromTile.MatchPiece;
        fromTile.SetCurrentMatchPiece(toTile.MatchPiece, PieceMovementType.Move);
        toTile.SetCurrentMatchPiece(piece, PieceMovementType.Move);

        StartCoroutine(WaitForBoardReady(callback));
    }

    void MoveToWrongPosition(BoardTile tile1, BoardTile tile2, SimpleEvent callback = null)
    {
        tile1.MatchPiece.MoveToWrongPosition(tile2.Transform.position);
        tile2.MatchPiece.MoveToWrongPosition(tile1.Transform.position);

        StartCoroutine(WaitForBoardReady(callback));
    }

    void ClearBoard(FloatVector3Event callback = null)
    {
        StartCoroutine(WaitForBoardReady(() =>
        {
            KillPieces(_board.Cast<BoardTile>().ToList(), callback);
        }));
    }

    void KillPieces(List<BoardTile> tiles, FloatVector3Event callback = null)
    {
        _tipTimer = 0f;

        if (_tileTip != null)
            _tileTip.MatchPiece.ToggleTip(false);

        Vector3 tilesCenter = Vector3.zero;
        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].MatchPiece.KillPiece(tiles[i].Transform.position);
            tiles[i].SetCurrentMatchPiece(null, PieceMovementType.Drop);

            tilesCenter += tiles[i].Transform.position;
        }

        callback?.Invoke(tiles.Count, tilesCenter / tiles.Count);
    }

    void TriggerScore(float pieceAmount, Vector3 piecesCenter)
    {
        Events.OnPieceKill?.Invoke(pieceAmount, piecesCenter);
        FillBoard();
    }

    IEnumerator WaitForBoardReady(SimpleEvent callback)
    {
        IEnumerable<BoardTile> collection = _board.Cast<BoardTile>();
        yield return new WaitUntil(() => collection.All(x => x.MatchPiece != null && x.MatchPiece.IsReady));
        callback?.Invoke();
    }
}