using System.Collections;
using UnityEngine;

public class MatchPiece : MonoBehaviour
{
    public PieceType pieceType;

    [Header("Move Config")]
    [SerializeField] float _moveToTileDuration = 1f;
    [SerializeField] float _moveToTileDelay = 0f;
    [SerializeField] AnimationCurve _moveToTileEasing;

    [Header("Drop Config")]
    [SerializeField] float _dropToTileBaseDuration = 0.25f;
    [SerializeField] float _dropToTileBaseDelay = 0.25f;
    [SerializeField] float _dropBounceBackDuration = 1f;
    [SerializeField] AnimationCurve _dropToTileEasing;

    Transform _transform;
    public Transform Transform 
    { 
        get
        {
            if (_transform == null) _transform = GetComponent<Transform>();
            return _transform; 
        } 
    }
    SpriteRenderer _spriteRenderer;
    Animation _animation;

    float _scale;
    bool _isAnimating = false;
    public bool IsReady { get => !_isAnimating; }

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _animation = GetComponent<Animation>();
    }

    public void MoveToTilePosition(Vector3 toPosition)
    {
        StartCoroutine(Move(toPosition, _moveToTileEasing, _moveToTileDuration, _moveToTileDelay, false));
    }

    public void DropToTilePosition(Vector3 toPosition)
    {
        float distance = Vector3.Distance(_transform.position, toPosition);

        StartCoroutine(Move(toPosition, _dropToTileEasing, _dropToTileBaseDuration * distance, _dropToTileBaseDelay * (BoardManager.Instance.BoardSize.y - distance + 1f), true));
    }

    public void MoveToWrongPosition(Vector3 toPosition)
    {
        Vector3 backToPosition = _transform.position;

        StartCoroutine(Move(Vector3.Lerp(toPosition, backToPosition, 0.5f), _moveToTileEasing, _moveToTileDuration / 2f, _moveToTileDelay, false, () =>
        {
            StartCoroutine(Move(backToPosition, _moveToTileEasing, _moveToTileDuration / 2f, 0f));
        }));
    }

    IEnumerator Move(Vector3 toPosition, AnimationCurve easingCurve, float duration, float delay, bool overshoot = false, SimpleEvent callback = null)
    {
        _isAnimating = true;

        yield return new WaitForSeconds(delay);

        float offsetOvershootTime = 0f;
        float time = 0f;
        Vector3 fromPosition = _transform.position;

        while (true)
        {
            time += Time.deltaTime;
            
            _transform.position = Vector3.LerpUnclamped(fromPosition, toPosition, easingCurve.Evaluate(offsetOvershootTime + time / duration));

            if (time >= duration)
            {
                if (overshoot)
                {
                    overshoot = false;
                    duration = _dropBounceBackDuration;
                    offsetOvershootTime = duration;
                    time = 0f;
                }
                else
                    break;
            }

            yield return null;
        }

        _transform.position = toPosition;

        _isAnimating = false;
        callback?.Invoke();
    }

    public void ToggleSelectedPiece(bool toggle)
    {
        if (toggle)
        {
            _spriteRenderer.sortingOrder = 10;
            _animation.Play("Piece_Selected");
        }
        else
        {
            _spriteRenderer.sortingOrder = 0;
            _animation.Stop();
            _transform.localScale = BoardManager.Instance.PieceSize;
            _transform.localRotation = Quaternion.identity;
        }
    }

    public void KillPiece(Vector3 position)
    {
        ObjectPooler.Instance.GetObject($"{pieceType}Particle", (Transform trans) =>
        {
            trans.position = position;
            trans.SetParent(BoardManager.Instance.particlesContainer);
            trans.gameObject.SetActive(true);
        });

        gameObject.SetActive(false);
    }
}