using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// 通过Resources同步异步加载资源的方法
/// </summary>
public class ResourcesResLoader : Singleton<ResourcesResLoader>
{
    private ResourcesResLoader() { }

    /// <summary>
    /// 同步加载Resources资源至内存中
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="uri">相对Resources目录的rui</param>
    /// <returns>加载好的资源</returns>
    public T LoadAsset<T>(string uri) where T : Object
    {
        return Resources.Load<T>(uri);
    }

    /// <summary>
    /// 异步加载Resources资源至内存中
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="uri">相对Resources目录的rui</param>
    /// <param name="OnProgressChanged">加载进度的更新事件</param>
    /// <param name="cancellationToken">取消加载的标记</param>
    /// <returns>加载好的资源</returns>
    public async UniTask<T> LoadAssetAsync<T>(string uri, UnityAction<float> OnProgressChanged = null,
        CancellationToken cancellationToken = default) where T : Object
    {
        System.IProgress<float> progress = null;
        if (OnProgressChanged != null) progress = Progress.CreateOnlyValueChanged<float>((handler) => { OnProgressChanged(handler); });

        ResourceRequest rq = Resources.LoadAsync<T>(uri);
        await rq.ToUniTask(progress: progress, cancellationToken: cancellationToken);

        return rq.asset as T;
    }

    /// <summary>
    /// 从内存中卸载Resources资源
    /// </summary>
    public void UnloadAsset(Object assetToUnload)
    {
        Resources.UnloadAsset(assetToUnload);
    }

    /// <summary>
    /// 从内存中卸载所有当前未使用的资源（一般过场景时配合GC使用）
    /// </summary>
    /// <param name="OnProgressChanged">卸载进度的更新事件</param>
    /// <param name="cancellationToken">取消卸载的标记</param>
    public async UniTask UnloadUnusedAssets(UnityAction<float> OnProgressChanged = null,
        CancellationToken cancellationToken = default)
    {
        var progress = Progress.CreateOnlyValueChanged<float>((handler) => { OnProgressChanged(handler); });
        await Resources.UnloadUnusedAssets().ToUniTask(progress: progress, cancellationToken: cancellationToken);
    }
}
