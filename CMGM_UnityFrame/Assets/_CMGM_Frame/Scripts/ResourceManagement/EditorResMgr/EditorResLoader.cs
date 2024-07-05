using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorResLoader : Singleton<EditorResLoader>
{
    private EditorResLoader() { }

    /// <summary>
    /// 在编辑器模式下加载AbRes文件夹下的资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="uri">相对于AbRes文件夹的uri</param>
    /// <returns>加载好的资源</returns>
    public T LoadResInEditor<T>(string uri) where T : Object
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<T>(uri);
#else
        return null;
#endif
    }

    //TODO:图集加载、其他编辑器资源加载

}
