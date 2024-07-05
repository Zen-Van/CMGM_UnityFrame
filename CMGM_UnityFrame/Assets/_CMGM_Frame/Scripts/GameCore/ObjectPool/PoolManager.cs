using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    Dictionary<string, PoolStack> poolDic = new Dictionary<string, PoolStack>();

    /// <summary>
    /// Editor模式开启方便测试，发布后关闭节约性能
    /// </summary>
    public static bool AUTO_LAYOUT_IN_HIERACHY
#if UNITY_EDITOR
     = true;
#else
     = false;
#endif

    private GameObject objPoolRoot;
    private PoolManager()
    {
        if (!AUTO_LAYOUT_IN_HIERACHY) return;
        if (objPoolRoot == null) objPoolRoot = new GameObject("Pools");
    }

    /// <summary>
    /// 搞来一个游戏对象（如果没有会进行加载和实例化）
    /// </summary>
    /// <param name="uri">
    /// 资源路径（{FrameRoot}/AbRes/ReuseableObject文件夹下相对路径）
    /// <para>仅能通过ResLoaderType.AssetBundle方式加载</para>
    /// </param>
    public async UniTask<GameObject> GetObj(string uri)
    {
        GameObject obj = null;

        if (poolDic.ContainsKey(uri) && poolDic[uri].Count > 0)
        {
            obj = poolDic[uri].Pop();
        }
        else
        {
            //加载资源(从可复用资源文件夹下加载)
            obj = GameObject.Instantiate
                (await ResManager.Instance.LoadAsync<GameObject>(Consts.Paths.ReuseableResPath + "/" + uri));
            //避免实例化出来的对象 默认会在名字后加一个（）
            obj.name = uri;
        }

        return obj;
    }

    public void PushObj(GameObject obj)
    {
        if (!poolDic.ContainsKey(obj.name))
            poolDic.Add(obj.name, new PoolStack(objPoolRoot, obj.name));
        poolDic[obj.name].Push(obj);
    }

    public void ClearPools()
    {
        poolDic.Clear(); //只要把字典清空，里面的stack断开引用就会都变成内存垃圾
        objPoolRoot = null; //把root的gameobject也断开

        //异步释放一下所有未被引用的内存资源
        ResManager.Instance.UnLoadAll_UnusedAssets().Forget();
    }
}
