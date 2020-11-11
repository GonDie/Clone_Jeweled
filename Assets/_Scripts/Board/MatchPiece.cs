using System.Collections;
using UnityEngine;

public class MatchPiece : PoolableObject
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
    Transform _childTrans;
    Animation _animation;

    float _scale;
    bool _isAnimating = false;
    public bool IsReady { get => !_isAnimating; }

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _childTrans = _spriteRenderer.GetComponent<Transform>();
        _animation = GetComponent<Animation>();
    }

    public void MoveToTilePosition(Vector3 toPosition)
    {
        StartCoroutine(Move(toPosition, _moveToTileEasing, _moveToTileDuration, _moveToTileDelay, false));
    }

    public void DropToTilePosition(Vector3 toPosition)
    {
        float distance = Vector3.Distance(_transform.position, toPosition);

        float duration = _dropToTileBaseDuration * distance;
        float delay = _dropToTileBaseDelay * (GameManager.Instance.BoardSize.y - distance + 1f);

        StartCoroutine(Move(toPosition, _dropToTileEasing, duration, delay, true));
        StartCoroutine(PlayAudio(duration + delay - 0.1f));
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

    IEnumerator PlayAudio(float delay)
    {
        yield return new WaitForSeconds(delay);

        SFXManager.Instance.PlaySFX(SFXType.DropPiece, true, 0.1f);
    }

    public void ToggleSelectedPiece(bool toggle)
    {
        if (toggle)
            PlayAnimation("Piece_Selected");
        else
            StopAnimation();
    }

    public void ToggleTip(bool toggle)
    {
        if (toggle)
            PlayAnimation("Piece_Tip");
        else
            StopAnimation();
    }

    void PlayAnimation(string anim)
    {
        StopAnimation();
        _spriteRenderer.sortingOrder = 10;
        _animation.Play(anim);
    }

    void StopAnimation()
    {
        _spriteRenderer.sortingOrder = 0;
        _animation.Stop();
        _transform.localScale = GameManager.Instance.PieceSize;
        _transform.localRotation = Quaternion.identity;

        _childTrans.localScale = Vector3.one * 0.4f;
        _childTrans.localRotation = Quaternion.identity;
    }

    public void KillPiece(Vector3 position)
    {
        ObjectPooler.Instance.GetObject($"{pieceType}Particle", (Transform trans) =>
        {
            trans.position = position;
            trans.SetParent(GameManager.Instance.ParticlesContainer);
            trans.gameObject.SetActive(true);
        });

        gameObject.SetActive(false);
    }
}