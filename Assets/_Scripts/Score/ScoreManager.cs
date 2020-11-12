using System.Collections;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    const float SCORE_UPDATE_SPEED = 150f;

    public Transform scoreContainer;
    [SerializeField] float _scorePerPiece = 50f;
    [SerializeField] float _baseTargetScore = 5000f;
    [SerializeField] float _levelScoreMultiplier = 1.5f;
    [SerializeField] float _streakDuration = 3f;

    float _baseLevelScore;
    float _targetScore;
    float _totalScore = 0f;
    float _currentScore = 0f;
    int _currentStreak = 0;

    Coroutine _streakResetCoroutine;

    private void Awake()
    {
        Events.OnGamePrepare += OnGamePrepare;
        Events.OnGameReset += OnGameReset;
        Events.OnPieceKill += OnPieceKill;
    }

    private void OnDestroy()
    {
        Events.OnGamePrepare -= OnGamePrepare;
        Events.OnPieceKill -= OnPieceKill;
    }

    void OnGamePrepare(int level)
    {
        _baseLevelScore = (_baseTargetScore * (level - 1 > 0 ? 1f : 0f)) + _baseTargetScore * (_levelScoreMultiplier * (level - 2 > 0 ? level - 2 : 0f));
        _targetScore = _baseTargetScore + (_baseTargetScore * (_levelScoreMultiplier * (level - 1)));

        Events.OnTargetScoreUpdate?.Invoke(_targetScore);
        Events.OnScoreUpdateToNextLevelPercent?.Invoke(0f);
    }

    void OnGameReset()
    {
        _totalScore = 0f;
        _currentScore = 0f;
        Events.OnScoreUpdate?.Invoke(_currentScore);
    }

    private void Update()
    {
        if (_currentScore >= _totalScore)
            return;

        _currentScore += SCORE_UPDATE_SPEED * Time.deltaTime;
        _currentScore = Mathf.Clamp(_currentScore, 0f, _totalScore);

        Events.OnScoreUpdate?.Invoke(_currentScore);
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

        Events.OnCameraShake?.Invoke(_currentStreak);

        Events.OnScoreUpdateToNextLevelPercent?.Invoke(Mathf.Abs(_baseLevelScore - _totalScore) / Mathf.Abs(_targetScore - _baseLevelScore));
        if (_totalScore >= _targetScore)
            Events.CallGameWon?.Invoke();
    }

    IEnumerator StreakResetCoroutine()
    {
        yield return new WaitForSeconds(_streakDuration);

        _currentStreak = 0;
        _streakResetCoroutine = null;
    }
}