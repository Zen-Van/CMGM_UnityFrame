using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public enum E_ResLoaderType
{
    Resources,
    AssetBundle,
    EditorDatabase,
    UnityWeb
}

/// <summary>
/// 资源在内存中的加载卸载管理
/// </summary>
public class ResManager : Singleton<ResManager>
{
    private ResManager() { }

    /// <summary>
    /// 是否真的通过AB包加载（false则在编辑器模式下通过编辑器加载方便测试，打包后永为true）
    /// </summary>
    private bool IS_TRUE_AB_LOAD = CmgmFrameSettings.Instance.IS_TRUE_AB_LOAD_IN_EDITOR || !Application.isEditor;

    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <typeparam name="T">加载资源类型</typeparam>
    /// <param name="uri">
    /// 资源路径
    /// <para>Resources下加载，传Resources下相对路径，不需要资源扩展名</para>
    /// <para>Editor或AB包加载，传Assets/开头的完整路径，需要资源扩展名</para>
    /// <para>不能通过web同步加载资源</para>
    /// </param>
    /// <param name="loader">加载资源的方式</param>
    /// <returns></returns>
    public T Load<T>(string uri, E_ResLoaderType loader = E_ResLoaderType.AssetBundle) where T : Object
    {
        switch (loader)
        {
            case E_ResLoaderType.Resources:
                return ResourcesResLoader.Instance.LoadAsset<T>(uri);
            case E_ResLoaderType.AssetBundle:
                if (IS_TRUE_AB_LOAD)
                {
                    AbResLoader.Instance.LoadRes<T>(uri);
                    return default;
                }
                else
                    break;
            case E_ResLoaderType.UnityWeb:
                //Web不能同步加载www
                CmgmLog.FrameLogError("Web获取资源不可以同步加载，请使用LoadAsync<T>()方法");
                break;
        }
        return EditorResLoader.Instance.LoadResInEditor<T>(uri);
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <typeparam name="T">加载资源类型</typeparam>
    /// <param name="uri">
    /// 资源路径
    /// <para>Resources下加载，传Resources下相对路径，不需要资源扩展名</para>
    /// <para>Editor或AB包加载，传Assets/开头的完整路径，需要资源扩展名</para>
    /// <para>Web加载，传带协议名的完整url，需要资源扩展名</para>
    /// </param>
    /// <param name="loader">
    /// 加载资源的方式
    /// <para>通过Web加载时，支持的类型仅有：string、byte[]、Texture、AssetBundle</para>
    /// </param>
    /// <param name="OnProgressChanged">进度回调事件</param>
    /// <param name="cancellationToken">加载取消标记</param>
    /// <returns>加载到的资源</returns>
    public async UniTask<T> LoadAsync<T>(string uri, E_ResLoaderType loader = E_ResLoaderType.AssetBundle,
        UnityAction<float> OnProgressChanged = null, CancellationToken cancellationToken = default) where T : Object
    {
        //Debug.Log(IS_TRUE_AB_LOAD);

        //await UniTask.Delay(1000);
        //CmgmLog.FrameLog_Negative("这地方delay了1秒模拟异步加载，之后记得删了，不然所有加载都会慢一秒");

        switch (loader)
        {
            case E_ResLoaderType.Resources:
                return await ResourcesResLoader.Instance.LoadAssetAsync<T>(uri, OnProgressChanged, cancellationToken);
            case E_ResLoaderType.UnityWeb:
                return await WebResLoader.Instance.LoadResAsync<T>(uri, OnProgressChanged, cancellationToken);
            case E_ResLoaderType.AssetBundle:
                if (IS_TRUE_AB_LOAD)
                {
                    return await AbResLoader.Instance.LoadResAsync<T>(uri, OnProgressChanged, cancellationToken);
                }
                else
                    break;
        }
        cancellationToken.ThrowIfCancellationRequested();
        //如果前面都没有返回成功，那么通过编辑器加载资源
        return EditorResLoader.Instance.LoadResInEditor<T>(uri);
    }

    /// <summary>
    /// 同步加载AB包镜像
    /// </summary>
    /// <param name="abName">ab包包名</param>
    public void LoadAB(string abName)
    {
        if (IS_TRUE_AB_LOAD)
            AbResLoader.Instance.LoadAB(abName);
    }
    /// <summary>
    /// 异步加载AB包镜像，TODO：后续可补充参数
    /// </summary>
    /// <param name="abName">ab包名</param>
    public async UniTask LoadABAsync(string abName)
    {
        if (IS_TRUE_AB_LOAD)
            await AbResLoader.Instance.LoadABAsync(abName);
    }


    #region 卸载与释放
    /// <summary>
    /// 显示的释放内存中的Asset对象（一个Destroy掉的Object短期内不会再被创建，就应该把他的内存也释放掉）
    /// </summary>
    public void UnLoad_Asset(Object assetToUnload)
    {
        ResourcesResLoader.Instance.UnloadAsset(assetToUnload);
    }
    /// <summary>
    /// 释放内存中所有没有被引用的Asset对象
    /// </summary>
    /// <param name="OnProgressChanged">卸载进度</param>
    /// <param name="cancellationToken">取消卸载标记</param>
    public async UniTask UnLoadAll_UnusedAssets(UnityAction<float> OnProgressChanged = null,
        CancellationToken cancellationToken = default)
    {
        await ResourcesResLoader.Instance.UnloadUnusedAssets(OnProgressChanged, cancellationToken);
    }
    /// <summary>
    /// 卸载内存中的AB包镜像
    /// </summary>
    /// <param name="unloadAllLoadedObjects">是否同时卸载被该AB包解压出的所有Asset</param>
    public void UnLoad_AssetBundle(string name, bool unloadAllLoadedObjects = false)
    {
        if (IS_TRUE_AB_LOAD)
            AbResLoader.Instance.UnLoadAB(name, unloadAllLoadedObjects);
    }
    /// <summary>
    /// 卸载内存中所有AB包镜像
    /// </summary>
    public void Clear_AssetBundle()
    {
        if (IS_TRUE_AB_LOAD)
            AbResLoader.Instance.ClearAB();
    }

    /// <summary>
    /// 触发GC分代回收内存中所有垃圾
    /// </summary>
    public void GC_Collect()
    {
        System.GC.Collect();
    }
    #endregion
}
