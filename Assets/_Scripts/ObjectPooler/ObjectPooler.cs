using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ObjectPooler : Singleton<ObjectPooler>
{
    Dictionary<string, List<GameObject>> _objectsPool;

    protected override void Awake()
    {
        base.Awake();

        _objectsPool = new Dictionary<string, List<GameObject>>();
    }

    public void GetObject<T>(string assetLabel, GenericEvent<T> callback) where T : MonoBehaviour
    {
        if (!_objectsPool.ContainsKey(assetLabel))
            _objectsPool.Add(assetLabel, new List<GameObject>());

        GameObject obj = _objectsPool[assetLabel].FirstOrDefault(x => !x.activeSelf);

        if(obj == null)
        {
            Addressables.InstantiateAsync(assetLabel).Completed += asyncHandler => 
            {
                _objectsPool[assetLabel].Add(asyncHandler.Result);
                callback?.Invoke(asyncHandler.Result.GetComponent<T>());
            };
            
            return;
        }

        callback(obj.GetComponent<T>());
    }
}