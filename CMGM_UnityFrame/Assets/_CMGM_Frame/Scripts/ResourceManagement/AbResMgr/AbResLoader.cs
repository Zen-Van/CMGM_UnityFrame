using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// TODO:AB包资源管理器
/// </summary>
public class AbResLoader : Singleton<AbResLoader>
{
    #region AB包管理相关
    private AbResLoader()
    {
        //初始化时获得所有的地址映射信息
        //AB包打包的时候，就把信息一并打包出来，然后直接在这里读取
        CmgmLog.TODO("AB包管理器若不在此处补充映射信息，任何AB资源都必须在加载完包体后再加载，不然会报错。" +
            "后续在此处补充完映射信息后，加载资源时即可自动加载所在包体");



        //把主包加载了，同步加载，没加载好这个类就给我等着！
        mainAB = AssetBundle.LoadFromFile(Consts.Paths.Ab_Bundle_Path + "/" + MainABName);
        manifest = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
    }

    /// <summary>
    /// 资源路径对应Ab路径的映射
    /// <para> 映射关系【资源Asset/完整路径】——> ab包名 </para>
    /// </summary>
    private Dictionary<string, string> assetPath_To_AbName = new Dictionary<string, string>();
    /// <summary>
    /// 内存中的AB包
    /// </summary>
    private Dictionary<string, AssetBundle> loadedDic = new Dictionary<string, AssetBundle>();

    /// <summary>
    /// 主包
    /// </summary>
    private AssetBundle mainAB = null;
    /// <summary>
    /// 主包名 方便修改
    /// </summary>
    private string MainABName
    {
        get
        {
#if UNITY_IOS
            return "IOS";
#elif UNITY_ANDROID
            return "Android";
#else
            return "PC";
#endif
        }
    }
    /// <summary>
    /// 加载包时临时存储信息
    /// </summary>
    private AssetBundleManifest manifest = null;

    /// <summary>
    /// 获得一个已加载的AB包,没加载就获得会报错
    /// </summary>
    public AssetBundle GetLoadedAB(string abName)
    {
        if (loadedDic.ContainsKey(abName))
            return loadedDic[abName];
        CmgmLog.FrameLogError("未在内存中发现AB包：" + abName);
        return null;
    }
    #endregion

    #region 加载资源函数
    /// <summary>
    /// 加载某AB包(同步)
    /// </summary>
    public void LoadAB(string abName)
    {
        abName = abName.ToLower();
        //abTemp先依次装各个依赖包，最后装加载的ab包
        AssetBundle abTemp = null;
        string[] strs = manifest.GetAllDependencies(abName);
        for (int i = 0; i < strs.Length; i++)
        {
            //依赖包们判断并加载
            if (!loadedDic.ContainsKey(strs[i]))
            {
                abTemp = AssetBundle.LoadFromFile(Consts.Paths.Ab_Bundle_Path + "/" + strs[i]);
                loadedDic.Add(strs[i], abTemp);
            }
        }

        //目标包判断并加载
        if (!loadedDic.ContainsKey(abName))
        {
            abTemp = AssetBundle.LoadFromFile(Consts.Paths.Ab_Bundle_Path + "/" + abName);
            loadedDic.Add(abName, abTemp);

            //更新一下资源的路径映射
            foreach (var assetName in abTemp.GetAllAssetNames())
            {
                assetPath_To_AbName[assetName] = abTemp.name;
                //CmgmLog.FrameLogPositive($"{assetName} --> {abTemp.name}");
            }
        }


    }
    /// <summary>
    /// 加载ab包（已加载则不重复加载）
    /// </summary>
    /// <param name="abName">ab包名</param>
    /// <returns>返回值</returns>
    public async UniTask LoadABAsync(string abName)
    {
        abName = abName.ToLower();
        //abTemp先依次装各个依赖包，最后装加载的ab包
        AssetBundle abTemp = null;
        string[] strs = manifest.GetAllDependencies(abName);
        for (int i = 0; i < strs.Length; i++)
        {
            //依赖包们判断并加载
            if (!loadedDic.ContainsKey(strs[i]))
            {
                loadedDic.Add(strs[i], null);//先占位置
                abTemp = await AssetBundle.LoadFromFileAsync(Consts.Paths.Ab_Bundle_Path + "/" + strs[i]);
                loadedDic[strs[i]] = abTemp;
            }
        }

        //目标包判断并加载
        if (!loadedDic.ContainsKey(abName))
        {
            loadedDic.Add(abName, null);//先占位置
            abTemp = await AssetBundle.LoadFromFileAsync(Consts.Paths.Ab_Bundle_Path + "/" + abName);

            if (abTemp == null) CmgmLog.FrameLogError($"AB包[{abName}]加载失败");
            loadedDic[abName] = abTemp;

            //更新一下资源的路径映射
            foreach (var assetName in abTemp.GetAllAssetNames())
            {
                assetPath_To_AbName[assetName] = abTemp.name;
                //CmgmLog.FrameLogPositive($"{assetName} --> {abTemp.name}");
            }
        }
    }


    public T LoadRes<T>(string url) where T : Object
    {
        url = url.ToLower().Replace("\\", "/");
        //加载AB包
        LoadAB(assetPath_To_AbName[url]);
        //加载并返回资源
        return loadedDic[assetPath_To_AbName[url]].LoadAsset<T>(url);
    }

    //传进来的Uri是Assets/开头的完整路径，在内部需要通过ab包包名，和包内路径来加载对应资源
    public async UniTask<T> LoadResAsync<T>(string url, UnityAction<float> OnProgressChanged = null,
        CancellationToken cancellationToken = default) where T : Object
    {
        url = url.ToLower().Replace("\\", "/");
        //Debug.Log(url);

        System.IProgress<float> progress = null;
        if (OnProgressChanged != null) progress = Progress.CreateOnlyValueChanged<float>((handler) => { OnProgressChanged(handler); });
        CmgmLog.TODO("AB包加载资源progress返回待优化，" +
            "应该根据需要加载的依赖包数量综合算每一个包体的加载时间按总百分比返回出去，" +
            "更好的优化是根据要加载的资源大小来返回进度，" +
            "现在忽略了AB包的加载，只会返回AB包完成后的资源加载进度。");

        //加载AB包
        await LoadABAsync(assetPath_To_AbName[url]);
        //加载并返回资源
        //Debug.Log($"从{loadedDic[assetPath_To_AbName[url]].name}这个AB包中，加载资源{url}");
        var request = loadedDic[assetPath_To_AbName[url]].LoadAssetAsync<T>(url);
        return await request.ToUniTask(progress: progress, cancellationToken: cancellationToken) as T;
    }

    //卸载AB包的方法
    public bool UnLoadAB(string name, bool unloadAllLoadedObjects = false)
    {
        if (!loadedDic.ContainsKey(name) || loadedDic[name] == null)
            return false;


        loadedDic[name].Unload(unloadAllLoadedObjects);
        loadedDic.Remove(name);
        return true;
    }

    //清空AB包的方法
    public void ClearAB()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        loadedDic.Clear();
        //卸载主包
        mainAB = null;
    }
    #endregion
}
