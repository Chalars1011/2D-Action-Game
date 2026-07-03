using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    // 字典
    public Dictionary<string, PoolData> pool = new Dictionary<string, PoolData>();

    private GameObject poolObj; // 对象池的父物体

    // 从对象池获取对象
    public GameObject GetObj(string name)
    {
        GameObject obj = null;
        if (pool.ContainsKey(name) && pool[name].poolQueue.Count > 0)
        {
            obj = pool[name].GetObj();
        }
        else
        {
            // 尝试从 Resources 加载预制体
            obj = Resources.Load<GameObject>(name);
            if (obj != null)
            {
                obj = GameObject.Instantiate(obj);
            }
            else
            {
                Debug.LogError($"Failed to load prefab: {name} from Resources.");
            }
        }
        if (obj != null)
        {
            obj.name = name;
        }
        return obj;
    }

    // 将对象放回对象池
    public void PushObj(string name, GameObject obj)
    {
        if (poolObj == null) { poolObj = new GameObject("Pool"); }
        if (!pool.ContainsKey(name))
        {
            pool.Add(name, new PoolData(obj, poolObj));
        }
        pool[name].PushObj(obj);
    }

    // 清空对象池
    public void Clear()
    {
        pool.Clear();
        poolObj = null;
    }
}