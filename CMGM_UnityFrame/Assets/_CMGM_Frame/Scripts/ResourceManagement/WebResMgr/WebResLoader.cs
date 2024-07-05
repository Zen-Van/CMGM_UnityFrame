using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class WebResLoader : Singleton<WebResLoader>
{
    private WebResLoader() { }

    /// <summary>
    /// 异步加载网络资源
    /// </summary>
    /// <typeparam name="T">加载资源类型，仅支持string、byte[]、Texture、AssetBundle</typeparam>
    /// <param name="url">带协议名的完整URL</param>
    /// <returns>返回的资源，为空则获取失败</returns>
    public async UniTask<T> LoadResAsync<T>(string url, UnityAction<float> OnProgressChanged = null,
        CancellationToken cancellationToken = default) where T : class
    {
        T obj = null;
        Type type = typeof(T);
        UnityWebRequest req = null;

        //获取不同种类的req
        if (type == typeof(string) || type == typeof(byte[]))
            req = UnityWebRequest.Get(url);
        else if (type == typeof(Texture))
            req = UnityWebRequestTexture.GetTexture(url);
        else if (type == typeof(AssetBundle))
            req = UnityWebRequestAssetBundle.GetAssetBundle(url);
        else
        {
            CmgmLog.FrameLogError($"未支持的资源下载类型：" + type);
            return null;
        }


        IProgress<float> progress = null;
        if (OnProgressChanged != null) progress = Progress.CreateOnlyValueChanged<float>((handler) => { OnProgressChanged(handler); });

        await req.SendWebRequest().ToUniTask
            (progress: progress, cancellationToken: cancellationToken); //等待req连接

        //通过不同种类的req获取资源
        if (req.result == UnityWebRequest.Result.Success)
        {
            if (type == typeof(string))
                obj = req.downloadHandler.text as T;
            else if (type == typeof(byte[]))
                obj = req.downloadHandler.data as T;
            else if (type == typeof(Texture))
                obj = DownloadHandlerTexture.GetContent(req) as T;
            else if (type == typeof(AssetBundle))
                obj = DownloadHandlerAssetBundle.GetContent(req) as T;
        }

        req.Dispose();//释放req连接
        return obj;
    }

}
