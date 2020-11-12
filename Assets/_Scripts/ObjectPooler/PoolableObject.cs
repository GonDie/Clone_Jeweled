using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    protected string _label;

    public void SetLabel(string label)
    {
        _label = label;
    }

    protected virtual void OnDisable()
    {
        if(ObjectPooler.Instance != null)
            ObjectPooler.Instance.ReturnObjectToPool(_label, gameObject);
    }
}