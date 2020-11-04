using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BoardManager : MonoBehaviour
{


    [SerializeField, SerializeReference]
    public ScriptableObject[] patterns;

    BoardTile[,] _board;
    Transform[] _spawnersTransform;

    MouseState _mouseState;
    bool _isPlaying;
    float _tipTimer = 0f;
    BoardTile _tileTip;

    void Awake()
    {
        Events.OnGameStart += OnGameStart;
        Events.OnGameWon += OnGameEnd;
        Events.OnGameTimeout += OnGameEnd;
        Events.OnMouseStateEvent += OnMouseState;
    }

    private void Start()
    {
        StartCoroutine(PrepareBoard());
    }

    void OnDestroy()
    {
        Events.OnGameStart -= OnGameStart;
        Events.OnGameWon -= OnGameEnd;
        Events.OnGameTimeout -= OnGameEnd;
        Events.OnMouseStateEvent -= OnMouseState;
    }

    private void Update()
    {
        if (!_isPlaying) return;

        _tipTimer += Time.deltaTime;
        if (_tipTimer >= GameManager.Instance.TipDelay && _tileTip != null && _mouseState == MouseState.Hovering)
        {
            _tipTimer = 0f;
            _tileTip.MatchPiece.ToggleTip(true);
        }
    }

    void OnMouseState(MouseState state)
    {
        _mouseState = state;
    }

    void OnGameStart()
    {
        _isPlaying = true;

        FillBoard();
    }

    void OnGameEnd()
    {
        _isPlaying = false;

        ClearBoard(() => StopAllCoroutines());
    }

    IEnumerator PrepareBoard()
    {
        Camera cam = Camera.main;
        Vector2 boardSize = GameManager.Instance.BoardSize;

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float boardBoundariesW = screenWidth * 0.9f;
        float boardBoundariesH = screenHeight * 0.8f;

        float scaleH = boardBoundariesH / boardBoundariesW;
        float scaleV = boardBoundariesW / boardBoundariesH;

        float pieceScale = (scaleH < scaleV ? boardBoundariesH / boardSize.y : boardBoundariesW / boardSize.x);
        Vector3 position = Vector3.zero;

        Vector2 pieceSize = Vector2.one * pieceScale;
        _board = new BoardTile[(int)boardSize.x, (int)boardSize.y];

        BoardTile tile = null;

        BoardTile up = null;
        BoardTile right = null;
        BoardTile down = null;
        BoardTile left = null;

        for (int row = 0; row < boardSize.y; row++)
        {
            for (int col = 0; col < boardSize.x; col++)
            {
                tile = null;
                Addressables.InstantiateAsync("BoardTile", GameManager.Instance.BoardContainer).Completed += handler =>
                {
                    tile = handler.Result.GetComponent<BoardTile>();
                };

                yield return new WaitUntil(() => tile != null);
                _board[row, col] = tile;
            }
        }

        for (int row = 0; row < boardSize.y; row++)
        {
            for (int col = 0; col < boardSize.x; col++)
            {
                position.x = (screenWidth / 2f) - (((pieceSize.x * boardSize.x) / 2f) - pieceSize.x / 2f) + pieceSize.x * col;
                position.y = (screenHeight / 2f) - (((pieceSize.y * boardSize.y) / 2f) - pieceSize.y / 2f) + pieceSize.y * row;
                position.z = 0f;

                if (col == 0) left = null;
                else left = _board[row, col - 1];

                if (col < boardSize.x - 1) right = _board[row, col + 1];
                else right = null;

                if (row == 0) down = null;
                else down = _board[row - 1, col];

                if (row < boardSize.y - 1) up = _board[row + 1, col];
                else up = null;

                _board[row, col].Init(GameManager.Instance.BoardContainer.position + cam.ScreenToWorldPoint(position), new Vector2(row, col), up, right, down, left);
            }

            for (int col = 0; col < boardSize.x; col++)
            {
                _board[row, col].SetSize(Vector2.one * Mathf.Abs(_board[0, 0].Transform.localPosition.x - _board[0, 1].Transform.localPosition.x));
            }
        }

        GameManager.Instance.PieceSize = Vector2.one * Mathf.Abs(_board[0, 0].Transform.localPosition.x - _board[0, 1].Transform.localPosition.x);

        Transform spawnTrans;
        Vector3 spawnPosition = cam.ScreenToWorldPoint(Vector3.up * (screenHeight + 100f));
        _spawnersTransform = new Transform[(int)boardSize.x];

        for (int col = 0; col < boardSize.x; col++)
        {
            spawnTrans = null;
            Addressables.InstantiateAsync("Spawner", GameManager.Instance.SpawnersContainer).Completed += handler =>
            {
                spawnTrans = handler.Result.GetComponent<Transform>();
            };

            yield return new WaitUntil(() => spawnTrans != null);

            spawnTrans.position = GameManager.Instance.BoardContainer.position + new Vector3(_board[0, col].Transform.localPosition.x, spawnPosition.y, 0f);
            _spawnersTransform[col] = spawnTrans;
        }
    }

    void FillBoard()
    {
        Events.OnMouseStateEvent(MouseState.Standby);

        for (int col = 0; col < (int)GameManager.Instance.BoardSize.x; col++)
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
        for(int row = 0; row < (int)GameManager.Instance.BoardSize.y; row++)
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
                    piece.Transform.SetParent(GameManager.Instance.PiecesContainer);
                    piece.Transform.position = _spawnersTransform[columnIndex].position;
                    piece.Transform.localScale = GameManager.Instance.PieceSize;
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
        for (int row = 0; row < (int)GameManager.Instance.BoardSize.x; row++)
        {
            for (int col = 0; col < (int)GameManager.Instance.BoardSize.x; col++)
            {
                tiles = CheckTileMatches(_board[row, col], null, _board[row, col].MatchPiece.pieceType);
                if (tiles.Count >= GameManager.Instance.MatchThreshold)
                {
                    matches.AddRange(tiles.Where(x => !matches.Contains(x)));
                }
            }
        }

        if (matches.Count >= GameManager.Instance.MatchThreshold)
        {
            SFXManager.Instance.PlaySFX(SFXType.MatchPiece);
            KillPieces(matches);
            TriggerScore(matches);
        }
        else
        {
            _tileTip = null;
            _tileTip = GetFirstPossibleMatch();
            if (_tileTip != null)
                Events.OnMouseStateEvent(MouseState.Hovering);
            else
                ClearBoard(() => FillBoard());
        }
    }

    public List<BoardTile> CheckTileMatches(BoardTile tile, BoardTile ignoreOther, PieceType type)
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

    BoardTile GetFirstPossibleMatch()
    {
        BoardTile tile = null;
        for (int row = 0; row < GameManager.Instance.BoardSize.y; row++)
        {
            for (int col = 0; col < GameManager.Instance.BoardSize.x; col++)
            {
                for (int i = 0; i < (int)Direction.Count; i++)
                {
                    if (_board[row, col].GetNeighbor((Direction)i) != null && 
                        CheckTileMatches(_board[row, col].GetNeighbor((Direction)i), _board[row, col], _board[row, col].MatchPiece.pieceType).Count >= GameManager.Instance.MatchThreshold)
                    {
                        tile = _board[row, col];
                        return tile;
                    }
                }
            }
        }

        return tile;
    }

    public void ExchangeTilePieces(BoardTile fromTile, BoardTile toTile, SimpleEvent callback = null)
    {
        SFXManager.Instance.PlaySFX(SFXType.SwapPiece);

        Events.OnMouseStateEvent(MouseState.Standby);

        MatchPiece piece = fromTile.MatchPiece;
        fromTile.SetCurrentMatchPiece(toTile.MatchPiece, PieceMovementType.Move);
        toTile.SetCurrentMatchPiece(piece, PieceMovementType.Move);

        StartCoroutine(WaitForBoardReady(callback));
    }

    public void MoveToWrongPosition(BoardTile tile1, BoardTile tile2, SimpleEvent callback = null)
    {
        tile1.MatchPiece.MoveToWrongPosition(tile2.Transform.position);
        tile2.MatchPiece.MoveToWrongPosition(tile1.Transform.position);

        StartCoroutine(WaitForBoardReady(callback));
    }

    void ClearBoard(SimpleEvent callback = null)
    {
        StartCoroutine(WaitForBoardReady(() =>
        {
            KillPieces(_board.Cast<BoardTile>().ToList(), callback);
        }));
    }

    public void KillPieces(List<BoardTile> tiles, SimpleEvent callback = null)
    {
        _tipTimer = 0f;

        if (_tileTip != null)
            _tileTip.MatchPiece.ToggleTip(false);

        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].MatchPiece.KillPiece(tiles[i].Transform.position);
            tiles[i].SetCurrentMatchPiece(null, PieceMovementType.Drop);
        }

        callback?.Invoke();
    }

    public void TriggerScore(List<BoardTile> tiles)
    {
        Vector3 tilesCenter = Vector3.zero;

        for (int i = 0; i < tiles.Count; i++)
        {
            tilesCenter += tiles[i].Transform.position;
        }

        TriggerScore(tiles.Count, tilesCenter / tiles.Count);
    }

    void TriggerScore(float pieceAmount, Vector3 piecesCenter)
    {
        Events.OnPieceKill?.Invoke(pieceAmount, piecesCenter);
        FillBoard();
    }

    public void TriggerOnBoardReady(SimpleEvent callback)
    {
        StartCoroutine(WaitForBoardReady(callback));
    }

    IEnumerator WaitForBoardReady(SimpleEvent callback)
    {
        IEnumerable<BoardTile> collection = _board.Cast<BoardTile>();
        yield return new WaitUntil(() => collection.All(x => x.MatchPiece != null && x.MatchPiece.IsReady));
        callback?.Invoke();
    }
}