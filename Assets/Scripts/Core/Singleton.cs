using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> :MonoBehaviour where T : Singleton<T>
//是一个泛型类型约束，它规定了类型参数T必须是继承自Singleton<T>本身的类型，确保了使用这个泛型单例模式时类型的一致性和正确性。
{
       private static T instance;


//这是一个公共静态属性，提供了外部获取单例实例的途径。外部代码可以通过Singleton<T>.Instance的方式来获取这个单例类的唯一实例
    public static T  Instance
    {
        get { return instance; }
    }
    protected virtual void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = (T)this;
        }
    }


//外部代码可以通过Singleton<T>.IsInitialized来检查单例是否已经可用，方便在使用单例之前进行必要的状态判断。
    public static bool IsInitialized
    {
        get { return instance != null; }
    }
    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
