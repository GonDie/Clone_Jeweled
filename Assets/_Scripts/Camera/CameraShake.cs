using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float shakeStrenght = 0.1f;
    public float shakeDuration = 0.5f;
    public int maxMagnitude = 5;

    Transform _transform;
    Coroutine _shakeRoutine;

    Vector3 _originPosition;

    void Awake()
    {
        _transform = GetComponent<Transform>();
        _originPosition = _transform.position;

        Events.OnCameraShake += OnCameraShake;
    }

    void DoShake(int magnitude)
    {
        if (_shakeRoutine != null)
            StopCoroutine(_shakeRoutine);

        _shakeRoutine = StartCoroutine(_DoShake(magnitude));
    }

    IEnumerator _DoShake(int magnitude)
    {
        float time = 0f;

        while(true)
        {
            time += Time.deltaTime;

            _transform.position = _originPosition + Random.insideUnitSphere * shakeStrenght * Mathf.Clamp(magnitude, 0, maxMagnitude);

            if (time >= shakeDuration)
                break;

            yield return new WaitForSeconds(0.05f);
        }

        _transform.position = _originPosition;
        _shakeRoutine = null;
    }

    void OnCameraShake(int magnitude)
    {
        DoShake(magnitude);
    }
}