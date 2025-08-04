using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 自动单例，不允许手动挂载或代码调用创建
/// </summary>
public class SingletonAutoMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject();
                obj.name = typeof(T).Name;
                instance = obj.AddComponent<T>();

                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    //支持手动挂载，如果没有手动挂载则自动生成
    protected virtual void Awake()
    {
        if (instance != null)
        {
            DestroyImmediate(this);
            return;
        }
        instance = this as T;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 不等到懒加载的提前加载方法
    /// </summary>
    public void Init() { } //为空即可
}
