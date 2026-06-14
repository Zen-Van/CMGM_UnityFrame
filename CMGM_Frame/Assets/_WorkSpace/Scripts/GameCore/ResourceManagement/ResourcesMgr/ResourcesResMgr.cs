using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class ResourcesResMgr : Singleton<ResourcesResMgr>
{
    private ResourcesResMgr() { }

    /// <summary>
    /// 同步加载Resources资源至内存中
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="uri">相对Resources目录的uri</param>
    /// <returns>加载好的资源</returns>
    public T LoadAsset<T>(string uri) where T : Object
    {
        return Resources.Load<T>(uri);
    }

    /// <summary>
    /// 异步加载Resources资源至内存中
    /// <para>注意：ResourceRequest没有原生取消机制。取消加载仅能取消异步等待，Unity仍会在后台继续加载资源。</para>
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="uri">相对Resources目录的uri</param>
    /// <param name="OnProgressChanged">加载进度的更新事件</param>
    /// <param name="cancellationToken">取消加载标记</param>
    /// <returns>加载好的资源</returns>
    public async UniTask<T> LoadAssetAsync<T>(string uri,
        UnityAction<float> OnProgressChanged = null,
        CancellationToken cancellationToken = default) where T : Object
    {
        var progress = OnProgressChanged != null ? Progress.CreateOnlyValueChanged<float>(p => OnProgressChanged(p)) : null;

        ResourceRequest request = Resources.LoadAsync<T>(uri);
        await request.ToUniTask(progress: progress, cancellationToken: cancellationToken);

        return request.asset as T;
    }

    /// <summary>
    /// 从内存中卸载Resources资源
    /// </summary>
    public void UnloadAsset(Object assetToUnload)
    {
        Resources.UnloadAsset(assetToUnload);
    }

    /// <summary>
    /// 从内存中卸载所有当前未被引用的资源（配合过场景时GC使用）
    /// </summary>
    /// <param name="OnProgressChanged">卸载进度</param>
    /// <param name="cancellationToken">取消卸载标记</param>
    public async UniTask UnloadUnusedAssets(
    UnityAction<float> OnProgressChanged = null,
    CancellationToken cancellationToken = default)
    {
        // 按需创建Progress实例（仅在需要时分配内存）
        var progress = OnProgressChanged != null ? Progress.CreateOnlyValueChanged<float>(p => OnProgressChanged(p)) : null;

        AsyncOperation request = Resources.UnloadUnusedAssets();
        await request.ToUniTask(progress: progress, cancellationToken: cancellationToken);
    }
}