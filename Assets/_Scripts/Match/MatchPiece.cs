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

    bool _isAnimating = false;
    public bool IsReady { get => !_isAnimating; }

    public void MoveToTilePosition(Vector3 toPosition)
    {
        StartCoroutine(Move(toPosition, _moveToTileEasing, _moveToTileDuration, _moveToTileDelay, false));
    }

    public void DropToTilePosition(Vector3 toPosition)
    {
        float distance = Vector3.Distance(_transform.position, toPosition);

        StartCoroutine(Move(toPosition, _dropToTileEasing, _dropToTileBaseDuration * distance, _dropToTileBaseDelay * (BoardManager.Instance.BoardSize.y - distance + 1f), true));
    }

    IEnumerator Move(Vector3 toPosition, AnimationCurve easingCurve, float duration, float delay, bool overshoot = false)
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
    }
}