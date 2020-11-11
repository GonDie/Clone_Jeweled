using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ObjectPooler : Singleton<ObjectPooler>
{
    Dictionary<string, Queue<GameObject>> _objectsPool;

    protected override void Awake()
    {
        base.Awake();

        _objectsPool = new Dictionary<string, Queue<GameObject>>();
    }

    public void GetObject<T>(string assetLabel, GenericEvent<T> callback)
    {
        if (!_objectsPool.ContainsKey(assetLabel))
            _objectsPool.Add(assetLabel, new Queue<GameObject>());

        GameObject obj = null;
        
        if(_objectsPool[assetLabel].Count > 0)
            obj = _objectsPool[assetLabel].Dequeue();

        if(obj == null)
        {
            Addressables.InstantiateAsync(assetLabel).Completed += asyncHandler => 
            {
                asyncHandler.Result.GetComponent<PoolableObject>().SetLabel(assetLabel);
                callback?.Invoke(asyncHandler.Result.GetComponent<T>());
            };
            
            return;
        }

        callback(obj.GetComponent<T>());
    }

    public void ReturnObjectToPool(string label, GameObject obj)
    {
        _objectsPool[label].Enqueue(obj);
    }
}