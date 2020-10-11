using System.Collections;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    const float SCORE_UPDATE_SPEED = 150f;

    public Transform scoreContainer;
    [SerializeField] float _scorePerPiece = 50f;
    [SerializeField] float _streakDuration = 3f;

    float _totalScore = 0f;
    float _currentScore = 0f;
    int _currentStreak = 0;

    Coroutine _streakResetCoroutine;

    private void Awake()
    {
        Events.OnPieceKill += OnPieceKill;
    }

    private void OnDestroy()
    {
        Events.OnPieceKill -= OnPieceKill;
    }

    private void Update()
    {
        _currentScore += SCORE_UPDATE_SPEED * Time.deltaTime;
        _currentScore = Mathf.Clamp(_currentScore, 0f, _totalScore);

        Events.OnScoreUpdate.Invoke(_currentScore);
    }

    private void OnPieceKill(float piecesKilled, Vector3 position)
    {
        if (_streakResetCoroutine != null)
            StopCoroutine(_streakResetCoroutine);

        _streakResetCoroutine = StartCoroutine(StreakResetCoroutine());

        _currentStreak++;
        _totalScore += piecesKilled * _scorePerPiece * _currentStreak;

        ObjectPooler.Instance.GetObject("ScorePopup", (ScorePopup score) =>
        {
            score.Transform.SetParent(scoreContainer);
            score.UpdateTextAndShow(position, piecesKilled * _scorePerPiece, _currentStreak);
        });

        CameraShake.Instance.DoShake(_currentStreak);
    }

    IEnumerator StreakResetCoroutine()
    {
        yield return new WaitForSeconds(_streakDuration);

        _currentStreak = 0;
        _streakResetCoroutine = null;
    }
}
