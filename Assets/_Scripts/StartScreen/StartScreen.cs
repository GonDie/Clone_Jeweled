using System.Collections;
using UnityEngine;

public class StartScreen : MonoBehaviour
{
    public float fadeOutDuration = 1f;

    CanvasGroup _cg;

    private void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
    }

    public void StartGame()
    {
        _cg.blocksRaycasts = false;
        _cg.interactable = false;

        StartCoroutine(FadeScreenOut());
    }

    IEnumerator FadeScreenOut()
    {
        float time = 0f;

        while(true)
        {
            time += Time.deltaTime;
            _cg.alpha = Mathf.Lerp(1f, 0f, time / fadeOutDuration);

            if (time >= fadeOutDuration)
                break;

            yield return null;
        }

        GameManager.Instance.StartGame();
    }
}
