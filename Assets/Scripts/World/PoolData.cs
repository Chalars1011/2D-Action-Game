using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolData
{
    // 存储对象池的父节点
    public GameObject fatherObj;
    // 存储对象的队列
    public Queue<GameObject> poolQueue;

    // 初始化
    public PoolData(GameObject obj, GameObject poolObj)
    {
        fatherObj = new GameObject(obj.name);
        fatherObj.transform.parent = poolObj.transform;
        poolQueue = new Queue<GameObject>();
        PushObj(obj); // 初始化时将对象放入队列
    }

    // 从对象池中获取对象
    public GameObject GetObj()
    {
        GameObject obj = poolQueue.Dequeue();
        obj.SetActive(true);
        obj.transform.parent = null;
        return obj;
    }

    // 将对象放回对象池
    public void PushObj(GameObject obj)
    {
        // 先将对象设置为失活状态
        obj.SetActive(false);
        // 再将对象加入队列
        poolQueue.Enqueue(obj);
        // 最后设置对象的父物体
        obj.transform.parent = fatherObj.transform;
    }
}