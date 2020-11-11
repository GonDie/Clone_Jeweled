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
        ObjectPooler.Instance.ReturnObjectToPool(_label, gameObject);
    }
}
