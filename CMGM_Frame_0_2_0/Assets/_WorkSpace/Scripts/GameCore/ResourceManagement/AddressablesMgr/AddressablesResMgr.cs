using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

//封装句柄，优化了装箱拆箱的情况
public class AddressablesResInfo
{
    public AsyncOperationHandle handle;
    public uint refCount;

    public AddressablesResInfo(AsyncOperationHandle handle, uint refCount = 1)
    {
        this.handle = handle;
        this.refCount = refCount;
    }
}


public class AddressablesResMgr : Singleton<AddressablesResMgr>
{
    private AddressablesResMgr() { }
    // 资源缓存
    private readonly ConcurrentDictionary<string, AddressablesResInfo> _assetCache = new();

    // 异步加载锁：防止重复加载同一资源
    private readonly ConcurrentDictionary<string, UniTask<AddressablesResInfo>> _pendingTasks = new();

    //--------------------------------------------------
    // 核心加载方法（UniTask 实现）
    //--------------------------------------------------

    /// <summary>
    /// 异步加载资源（带缓存和引用计数）
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="uri">资源地址（ HotRes文件夹下相对路径,需要带后缀 ）</param>
    public async UniTask<T> LoadAssetAsync<T>(string uri) where T : class
    {
        AsyncOperationHandle<T> handle = default;

        string assetAddress = $"{Consts.Paths.HotRes}/{uri}";

        // 1. 从缓存中快速返回
        if (_assetCache.TryGetValue(assetAddress, out var cacheEntry))
        {
            cacheEntry.refCount++;
            handle = cacheEntry.handle.Convert<T>();
            return handle.Result;
        }

        // 2. 检查是否正在加载中，避免重复请求
        if (_pendingTasks.TryGetValue(assetAddress, out var pendingTask))
        {
            handle = (await pendingTask).handle.Convert<T>();
            return handle.Result;
        }

        // 3. 创建新的加载任务并加入队列
        var loadTask = LoadAssetInternal<T>(assetAddress);
        _pendingTasks[assetAddress] = loadTask;

        try
        {
            var asset = (await loadTask).handle.Convert<T>().Result;
            return asset;
        }
        finally
        {
            _pendingTasks.TryRemove(assetAddress, out _);
        }
    }

    private async UniTask<AddressablesResInfo> LoadAssetInternal<T>(string assetAddress) where T : class
    {
        _assetCache[assetAddress] = new AddressablesResInfo(Addressables.LoadAssetAsync<T>(assetAddress), 1);
        var handle = _assetCache[assetAddress].handle;
        await handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return _assetCache[assetAddress];
        }
        else
        {
            Addressables.Release(handle);
            throw new Exception($"加载失败: {assetAddress}, 错误: {handle.OperationException}");
        }
    }

    //--------------------------------------------------
    // 资源释放方法
    //--------------------------------------------------

    /// <summary>
    /// 释放资源（引用计数-1，计数归零时c才能销毁）
    /// </summary>
    /// <param name="uri">资源地址（ HotRes文件夹下相对路径 ）</param>
    /// <remarks>注意：此方法不会立即释放资源，而是减少引用计数</remarks>
    /// <remarks>如果引用计数为0，则会立即销毁资源</remarks>
    public void ReleaseAsset(string uri)
    {
        string assetAddress = $"{Consts.Paths.HotRes}/{uri}";

        if (_assetCache.TryGetValue(assetAddress, out var entry))
        {
            uint newCount = entry.refCount - 1;
            if (newCount <= 0)
            {
                if (_assetCache.TryRemove(assetAddress, out var removedEntry))
                {
                    Addressables.Release(removedEntry.handle.Result);
                }
            }
            else
            {
                entry.refCount = newCount;
            }
        }
    }

    /// <summary>
    /// 销毁所有未被引用的资源（引用计数为0的资源）
    /// </summary>
    /// <remarks>注意：此方法会立即释放所有引用计数为0的资源</remarks>
    /// <remarks>如果引用计数不为0，则不会释放资源</remarks>
    public int ReleaseUnusedAssets()
    {
        int releasedCount = 0;
        var addressesToRelease = new List<string>();

        // 第一步：收集所有引用计数为0的资源地址
        foreach (var pair in _assetCache)
        {
            if (pair.Value.refCount <= 0)
            {
                addressesToRelease.Add(pair.Key);
            }
        }

        // 第二步：批量释放资源
        foreach (var address in addressesToRelease)
        {
            if (_assetCache.TryRemove(address, out var entry))
            {
                Addressables.Release(entry.handle.Result);
                releasedCount++;
            }
        }

        return releasedCount;
    }

    /// <summary>
    /// 强制释放所有资源（无视引用计数，慎用！）
    /// </summary>
    public void ReleaseAllAssets()
    {
        foreach (var pair in _assetCache)
        {
            Addressables.Release(pair.Value.handle.Result);
        }
        _assetCache.Clear();
        _pendingTasks.Clear();

        Debug.LogWarning("已强制释放所有资源，可能引发后续加载异常！");
    }

    /// <summary>
    /// 异步释放未使用的资源（可选）
    /// 注意：此方法会在主线程中执行，确保不会阻塞游戏逻辑
    /// 适用于需要在游戏运行时动态释放资源的场景
    /// </summary>
    public async UniTask ReleaseUnusedAssetsAsync()
    {
        await UniTask.SwitchToMainThread(); // 确保在主线操作
        ReleaseUnusedAssets();

        // 可选：等待一帧确保资源释放完成
        await UniTask.Yield();
    }

    //--------------------------------------------------
    // 高级功能扩展
    //--------------------------------------------------

    private async UniTask PreloadSingleAssetAsync(string address)
    {
        try
        {
            await LoadAssetAsync<object>(address);
        }
        catch (Exception e)
        {
            Debug.LogError($"预加载失败: {address}\n{e}");
        }
    }

    /// <summary>
    /// 批量预加载资源（加载场景进度条）
    /// </summary>
    /// <param name="uris">资源地址列表（ HotRes文件夹下相对路径 ）</param>
    public async UniTask<bool> PreloadAssetsAsync(IEnumerable<string> uris,
        IProgress<float> progress = null)
    {
        IEnumerable<string> addresses = uris.Select(uri => $"{Consts.Paths.HotRes}/{uri}");
        var addressList = addresses.ToList(); // 转换为List避免多次枚举
        int total = addressList.Count;
        int completed = 0;

        // 创建任务列表并添加完成回调
        var tasks = new List<UniTask>();
        foreach (var address in addressList)
        {
            tasks.Add(PreloadSingleAssetAsync(address).ContinueWith(() =>
            {
                Interlocked.Increment(ref completed); // 线程安全递增
                ReleaseAsset(address); // 预加载完成后立即释放资源(引用计数-1)
            }));
        }

        var allTasks = UniTask.WhenAll(tasks);

        // 进度报告循环
        while (allTasks.Status == UniTaskStatus.Pending)
        {
            progress?.Report((float)completed / total);
            await UniTask.Yield(); // 每帧更新一次进度
        }

        // 最终进度报告（确保到达100%）
        progress?.Report(1f);

        return allTasks.Status == UniTaskStatus.Succeeded;
    }

    /// <summary>
    /// 批量预加载资源（加载场景进度条）
    /// </summary>
    /// <param name="folderPath"> 资源文件夹（ HotRes文件夹下相对路径 ）</param>
    public async UniTask<bool> PreloadAssetsAsync(string folderPath, IProgress<float> progress = null)
    {
        // 构建完整的文件夹地址路径
        string fullFolderPath = $"{Consts.Paths.HotRes}/{folderPath}/";

        // 加载Addressables中所有资源的位置信息
        AsyncOperationHandle<IList<IResourceLocation>> locationHandle
            = Addressables.LoadResourceLocationsAsync(fullFolderPath, typeof(object));

        // 等待加载完成
        await locationHandle.Task;

        // 检查加载状态
        if (locationHandle.Status != AsyncOperationStatus.Succeeded)
        {
            CmgmLog.fError($"Failed to load resource locations for folder: {fullFolderPath}");
            Addressables.Release(locationHandle);
            return false;
        }

        // 获取文件夹下所有资源的地址
        List<string> addresses = locationHandle.Result
            .Where(loc => loc.PrimaryKey.StartsWith(fullFolderPath))
            .Select(loc => loc.PrimaryKey)
            .Distinct()
            .ToList();

        // 释放位置句柄
        Addressables.Release(locationHandle);

        // 如果没有找到资源
        if (addresses.Count == 0)
        {
            Debug.LogWarning($"No assets found in folder: {fullFolderPath}");
            progress?.Report(1f);
            return true;
        }

        // 创建预加载任务列表
        var tasks = new List<UniTask>();
        foreach (var address in addresses)
        {
            tasks.Add(PreloadSingleAssetAsync(address));
        }

        // 进度跟踪
        int total = tasks.Count;
        int completed = 0;

        // 创建组合任务
        var whenAllTask = UniTask.WhenAll(tasks).ContinueWith(() =>
        {
            foreach (var address in addresses)
            {
                ReleaseAsset(address);
            }
        });

        // 进度报告协程
        var progressTask = UpdateProgressAsync();

        // 等待所有任务完成
        await UniTask.WhenAll(whenAllTask, progressTask);

        return whenAllTask.Status == UniTaskStatus.Succeeded;

        // 本地函数：更新进度
        async UniTask UpdateProgressAsync()
        {
            while (completed < total)
            {
                progress?.Report((float)completed / total);
                await UniTask.Yield();
                completed = tasks.Count(t => t.Status == UniTaskStatus.Succeeded);
            }
            progress?.Report(1f);
        }
    }

}
