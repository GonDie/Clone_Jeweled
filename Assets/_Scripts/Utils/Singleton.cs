using UnityEngine;

public class Singleton<T> : MonoBehaviour
{
    static T _instance;
    public static T Instance { get => _instance; }

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = gameObject.GetComponent<T>();
    }

    protected virtual void OnDestroy()
    {
        _instance = default(T);
    }
}