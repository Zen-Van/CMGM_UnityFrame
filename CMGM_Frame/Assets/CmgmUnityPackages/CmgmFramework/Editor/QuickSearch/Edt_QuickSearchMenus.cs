using System;
using System.Diagnostics;
using System.IO;
using CMGM.Core;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 草木句萌 Quick Search：Project 跳转与系统文件夹打开。
/// </summary>
public static class Edt_QuickSearchMenus
{
    [MenuItem("Assets/CMGM Search/入口场景", false, -200)]
    static void SelectInitScenesPath()
    {
        PathSelect(Consts.Paths.HotScene + "/InitScene.unity");
    }

    [MenuItem("Assets/CMGM Search/工程配置文件", false, -178)]
    static void SelectFrameSettingPath()
    {
        PathSelect(Consts.Paths.Framework.Resources + "/CmgmFrameSettings.asset");
    }

    [MenuItem("草木句萌/Quick Search/打开：Assets （资源根目录）", false, 160)]
    static void OpenDataPath()
    {
        OpenFolder(Application.dataPath);
    }

    [MenuItem("草木句萌/Quick Search/打开：Streaming Assets （配置文件夹）", false, 160)]
    static void OpenStreamingAssetsPath()
    {
        if (!Directory.Exists(Application.streamingAssetsPath))
            Directory.CreateDirectory(Application.streamingAssetsPath);

        OpenFolder(Application.streamingAssetsPath);
    }

    [MenuItem("草木句萌/Quick Search/打开：Persistent Data （存档文件夹）", false, 160)]
    static void OpenPersistentDataPath()
    {
        OpenFolder(Application.persistentDataPath);
    }

    [MenuItem("草木句萌/Quick Search/打开：Temporary Cache （缓存文件夹）", false, 161)]
    static void OpenTemporaryCachePath()
    {
        OpenFolder(Application.temporaryCachePath);
    }

    [MenuItem("草木句萌/Quick Search/打开：Console Log （日志文件夹）", false, 161)]
    static void OpenConsoleLogPath()
    {
        OpenFolder(Path.GetDirectoryName(Application.consoleLogPath));
    }

    public static void OpenFolder(string folder)
    {
        folder = $"\"{folder}\"";
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                Process.Start("Explorer.exe", folder.Replace('/', '\\'));
                break;
            case RuntimePlatform.OSXEditor:
                Process.Start("open", folder);
                break;
            default:
                throw new Exception($"Not support open folder on '{Application.platform}' platform.");
        }
    }

    public static void PathSelect(string path)
    {
        UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        EditorGUIUtility.PingObject(obj);
        Selection.activeObject = obj;
    }
}
