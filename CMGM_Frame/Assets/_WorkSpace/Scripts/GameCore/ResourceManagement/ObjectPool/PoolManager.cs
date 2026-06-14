using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    // 对象池数据结构
    Dictionary<string, PoolStack> poolDic = new Dictionary<string, PoolStack>();
    // 资源引用跟踪：记录每个资源地址的加载次数（用于正确释放）
    private Dictionary<string, int> _assetRefCounts = new Dictionary<string, int>();

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
    /// 资源路径（ HotRes文件夹下相对路径 ）
    /// <para>通过aa包方式加载</para>
    /// </param>
    public async UniTask<GameObject> GetObj(string uri)
    {
        GameObject obj = null;

        // 从对象池获取
        if (poolDic.TryGetValue(uri, out var pool) && pool.Count > 0)
        {
            obj = pool.Pop();
        }
        // 需要新加载
        else
        {
            // 加载资源并记录引用
            var prefab = await AddressablesResMgr.Instance.LoadAssetAsync<GameObject>(uri);
            obj = GameObject.Instantiate(prefab);
            obj.name = uri;

            // 更新资源引用计数
            if (_assetRefCounts.ContainsKey(uri))
            {
                _assetRefCounts[uri]++;
            }
            else
            {
                _assetRefCounts[uri] = 1;
            }
        }

        return obj;
    }

    public void PushObj(GameObject obj)
    {
        if (!poolDic.ContainsKey(obj.name))
            poolDic.Add(obj.name, new PoolStack(objPoolRoot, obj.name));
        poolDic[obj.name].Push(obj);
    }

    /// <summary>
    /// 清理所有对象池及其关联资源
    /// </summary>
    public async UniTask ClearPoolsAsync()
    {
        // 1. 清空对象池
        poolDic.Clear();

        // 2. 释放所有关联的 Addressables 资源
        foreach (var kvp in _assetRefCounts)
        {
            string address = kvp.Key;
            int loadCount = kvp.Value;

            // 根据加载次数释放引用
            for (int i = 0; i < loadCount; i++)
            {
                AddressablesResMgr.Instance.ReleaseAsset(address);
            }
        }
        _assetRefCounts.Clear();

        // 3. 触发未使用资源释放（可选异步）
        await AddressablesResMgr.Instance.ReleaseUnusedAssetsAsync();
        await ResourcesResMgr.Instance.UnloadUnusedAssets();
    }
}
