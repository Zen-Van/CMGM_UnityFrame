using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CMGM.Core;
using UnityEditor;
using UnityEngine;

public class Edt_BaseUtils
{
    public const string EditorRoot = Consts.Paths.WorkSpace + "/Editor";

    #region 快速检索
    [MenuItem("Assets/CMGM Search/入口场景", false, -200)]
    static void SelectInitScenesPath()//s = s.Substring(0,s.Length - 1)
    {
        string path = "Assets/_CMGM_Frame/AbRes/Scenes/InitScene.unity";
        PathSelect(path);
    }

    [MenuItem("Assets/CMGM Search/工程配置文件", false, -178)]
    static void SelectFrameSettingPath()//s = s.Substring(0,s.Length - 1)
    {
        string path = Consts.Paths.WorkSpace + "/Resources/CmgmFrameSettings.asset";
        PathSelect(path);
    }


    //------------------------------------------------------
    [MenuItem("草木句萌/Quick Search/打开：Assets （资源根目录）", false, 160)]
    static void OpenDataPath()
    {
        OpenFolder(Application.dataPath);
    }
    [MenuItem("草木句萌/Quick Search/打开：Streaming Assets （配置文件夹）", false, 160)]
    static void OpenStreamingAssetsPath()
    {
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }
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
        //定位选中并激活
        EditorGUIUtility.PingObject(obj);
        Selection.activeObject = obj;
    }

    #endregion



}
