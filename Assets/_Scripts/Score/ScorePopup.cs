using System.Collections;
using TMPro;
using UnityEngine;

public class ScorePopup : MonoBehaviour
{
    const float ANIMATION_DURATION = 2f;

    Transform _transform;
    public Transform Transform { get => _transform; }

    [SerializeField] TMP_Text _textValue;
    [SerializeField] TMP_Text _textMultiplier;
    [SerializeField] Color[] _multiplierColors;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
    }

    public void UpdateTextAndShow(Vector3 position, float scoreValue, int scoreMultiplier)
    {
        gameObject.SetActive(true);
        StartCoroutine(OnAnimationEnd());

        _transform.position = position;
        _textValue.text = Mathf.Round(scoreValue).ToString();

        _textMultiplier.color = _multiplierColors[(scoreMultiplier - 1) % _multiplierColors.Length];
        _textMultiplier.text = $"x{scoreMultiplier.ToString()}";
    }

    IEnumerator OnAnimationEnd()
    {
        yield return new WaitForSeconds(ANIMATION_DURATION);

        gameObject.SetActive(false);
    }
}
