using System.Collections;
using UnityEngine;

public class VictoryScreen : BaseScreen
{
    [SerializeField] float _playAgainDelay;

    protected override void Awake()
    {
        base.Awake();

        Events.OnGameWon += OnGameWon;
    }

    private void OnDestroy()
    {
        Events.OnGameWon -= OnGameWon;
    }

    void OnGameWon()
    {
        ToggleScreen(true, () => StartCoroutine(WaitPlayAgain()));
    }

    IEnumerator WaitPlayAgain()
    {
        yield return new WaitForSeconds(_playAgainDelay);

        Events.CallPrepareGame?.Invoke();

        ToggleScreen(false, () => Events.CallStartGame?.Invoke());
    }
}
