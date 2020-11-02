using System.Collections;
using UnityEngine;

public class BaseScreen : MonoBehaviour
{
    public float fadeDelay = 0f;
    public float fadeDuration = 1f;

    protected CanvasGroup _cg;

    protected virtual void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
    }

    protected void ToggleScreen(bool toggle, SimpleEvent callback)
    {
        StartCoroutine(ToggleScreenRoutine(toggle, callback));
    }

    protected IEnumerator ToggleScreenRoutine(bool toggle, SimpleEvent callback)
    {
        _cg.interactable = toggle;
        _cg.blocksRaycasts = toggle;

        float time = 0f;

        yield return new WaitForSeconds(fadeDelay);

        while (true)
        {
            time += Time.deltaTime;
            _cg.alpha = Mathf.Lerp(toggle ? 0f : 1f, toggle ? 1f : 0f, time / fadeDuration);

            if (time >= fadeDuration)
                break;

            yield return null;
        }

        callback?.Invoke();
    }
}