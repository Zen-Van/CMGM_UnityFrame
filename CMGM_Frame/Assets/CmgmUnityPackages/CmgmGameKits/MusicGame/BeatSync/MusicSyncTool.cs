using CMGM.Audio;
using CMGM.Core;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;

public static class MusicSyncTool
{
    #region 音乐播放数据
    public static uint curBgmPlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
    public static uint curBgmEventId = 0;


    /// <summary>
    /// 正在播放的音乐的播放位置（单位：ms）
    /// </summary>
    /// <returns>播放位置（单位：ms）</returns>
    public static int CurBgmPosition
    {
        get
        {
            int bgmPosition;
            AkUnitySoundEngine.GetSourcePlayPosition(curBgmPlayingId, out bgmPosition);
            return bgmPosition;
        }
    }

    #endregion

    #region 节拍同步判定
    // 节拍判定窗口（单位：毫秒）。原在 WwiseAudioManager，编译边界2.7a 迁入音游侧自持。
    // TODO: 后续可改为按 BPM 自适应 / ScriptableObject 配置。
    public static int goodWindow = 150;
    public static int greatWindow = 100;
    public static int perfectWindow = 50;

    public enum BeatInputState { miss = 0, good = 1, great = 2, perfect = 3 }
    public static BeatInputState curBeatInputState = BeatInputState.miss;
    /// <summary>据下一拍的时长</summary>
    public static float BeatPercent { get;private set; }

    /// <summary>
    /// 当前判定的事件的角标
    /// </summary>
    private static int judgeIndex = 0;

    private static void RefreshCurBeatState()
    {
        //如果音乐指针在最后一个事件判定之后，则重置判定角标
        if (CurBgmPosition >= evtTimeList[evtTimeList.Count-1] + goodWindow)
        {
            judgeIndex = 0;
            BeatPercent = 0;
            curBeatInputState = BeatInputState.miss;
            return;
        }

        //赋值节拍器
        if (judgeIndex > 0)
            BeatPercent = ((float)CurBgmPosition - (float)evtTimeList[judgeIndex - 1]) /
                ((float)evtTimeList[judgeIndex] - (float)evtTimeList[judgeIndex - 1]);
        if (BeatPercent > 1) BeatPercent -= 1;

        //按区域判定节拍状态
        if (evtTimeList.Count == 0) return;
        int judgeEvtTime = evtTimeList[judgeIndex];
        if (CurBgmPosition < judgeEvtTime - goodWindow)
        {
            curBeatInputState = BeatInputState.miss;
        }
        else if (CurBgmPosition < judgeEvtTime - greatWindow)//从左侧进入good区域
        {
            curBeatInputState = BeatInputState.good;
        }
        else if (CurBgmPosition < judgeEvtTime - perfectWindow)//从左侧进入great区域
        {
            curBeatInputState = BeatInputState.great;
        }
        else if (CurBgmPosition < judgeEvtTime + perfectWindow)//从左侧进入perfect区域
        {
            curBeatInputState = BeatInputState.perfect;
        }
        else if (CurBgmPosition < judgeEvtTime + greatWindow)//从右侧离开perfect区域
        {
            curBeatInputState = BeatInputState.great;
        }
        else if (CurBgmPosition < judgeEvtTime + goodWindow)//从右侧离开great区域
        {
            curBeatInputState = BeatInputState.good;
        }
        else//从右侧离开good区域
        {
            curBeatInputState = BeatInputState.miss;
            if (judgeIndex < evtTimeList.Count - 1) judgeIndex++; //保证结尾时数组不越界，并保证数值锁定
        }
    }


    /// <summary>
    /// 单轨事件
    /// </summary>
    private static List<int> evtTimeList = new List<int>();
    public static void ActiveMusicBeatSync(string beatEvtListJsonPath)
    {
        //清空事件列表
        evtTimeList.Clear();
        //获取当前音乐需要被判定的事件列表
        LoadBeatEvtListFromJson(beatEvtListJsonPath);
        //将RefreshCurState()注册进Update()
        MonoMgr.Instance.AddUpdateListener(RefreshCurBeatState);

    }
    public static void DisableMusicBeatSync()
    {
        evtTimeList.Clear();
        //将RefreshCurState()从Update()中移除
        MonoMgr.Instance.RemoveUpdateListener(RefreshCurBeatState);
    }
    // 从JSON文件加载节拍映射数据
    private static List<int> LoadBeatEvtListFromJson(string beatEvtListJsonPath)
    {
        if (string.IsNullOrEmpty(beatEvtListJsonPath))
        {
            Debug.LogError("节拍映射JSON路径未设置!");
            return null;
        }

        if (!File.Exists(beatEvtListJsonPath))
        {
            Debug.LogError($"节拍映射文件不存在: {beatEvtListJsonPath}");
            return null;
        }

        try
        {
            string jsonContent = File.ReadAllText(beatEvtListJsonPath);
            BeatEvtListData beatMapData = JsonUtility.FromJson<BeatEvtListData>(jsonContent);

            // 将节拍时间添加到列表
            evtTimeList.AddRange(beatMapData.beatTimesMs);

            Debug.Log($"成功加载节拍映射数据，共 {beatMapData.beatTimesMs.Length} 个节拍点");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载节拍映射失败: {e.Message}");
        }

        return evtTimeList;
    }

    #endregion


    #region 带节拍同步的 BGM 播放（音游：组合通用音频原语 + 节拍同步）
    /// <summary>
    /// 播放 BGM 并开启节拍同步（音游用）。组合 WwiseAudioManager 的通用播放原语 + 本工具的节拍同步。
    /// </summary>
    /// <param name="eventName">Wwise 事件名</param>
    /// <param name="beatMapName">节拍图名（null 则只播放、不开节拍同步）</param>
    /// <param name="callbackFunc">自定义回调（null 用默认 MusicEventDefaultCallbackFunc）</param>
    public static uint PlayBgmWithBeatSync(string eventName, string beatMapName = null, AkCallbackManager.EventCallback callbackFunc = null)
    {
        curBgmEventId = AkUnitySoundEngine.GetIDFromString(eventName);

        curBgmPlayingId = WwiseAudioManager.Instance.PlayWwiseEventWithCallback(
            eventName, WwiseAudioManager.Instance.gameObject,
            AkCallbackType.AK_EnableGetMusicPlayPosition | AkCallbackType.AK_MusicSyncAll,
            callbackFunc == null ? MusicEventDefaultCallbackFunc : callbackFunc);

        if (beatMapName != null)
            ActiveMusicBeatSync(MusicGameConsts.RhythmMap_Path + $"/{beatMapName}/{beatMapName}_BeatEvtList.json");
        else
            DisableMusicBeatSync();

        return curBgmPlayingId;
    }

    /// <summary>停止带节拍同步的 BGM 并关闭节拍同步。</summary>
    public static void StopBgmWithBeatSync()
    {
        AkUnitySoundEngine.StopPlayingID(curBgmPlayingId);
        DisableMusicBeatSync();
    }
    #endregion


    #region 音乐通用回调接口（也可作为非通用回调的案例）
    /// <summary> 设置节拍参数 </summary>
    public static bool _isBarTriggered = false;
    public static int _beatIndex = 0;

    public static void MusicEventDefaultCallbackFunc(object inCookie, AkCallbackType inType, AkCallbackInfo inInfo)
    {
        switch (inType)
        {
            case AkCallbackType.AK_MusicSyncEntry:
                Debug.Log("【TestAudioCallback】 Received: AK_MusicSyncEntry");
                CmgmLog.fNormal($"Play_TestMusic的EventId为：{AkUnitySoundEngine.GetIDFromString("Play_TestMusic")}\n" +
                    $"当前播放的音乐的PlayingId为：{curBgmPlayingId}\n"+
                    $"当前播放的音乐的EventId为：{AkUnitySoundEngine.GetEventIDFromPlayingID(curBgmPlayingId)}");

                break;
            case AkCallbackType.AK_MusicSyncExit:
                Debug.Log("【TestAudioCallback】 Received: AK_MusicSyncExit");
                //重置节拍进度
                judgeIndex = 0;
                BeatPercent = 0;
                curBeatInputState = BeatInputState.miss;
                break;
            case AkCallbackType.AK_MusicSyncBar:
                if (!_isBarTriggered) // 防止同一帧重复处理 
                {
                    _isBarTriggered = true;
                    Debug.Log("【TestAudioCallback】 Received: AK_MusicSyncBar");
                    _beatIndex = 0;
                }
                break;
            case AkCallbackType.AK_MusicSyncBeat:
                if (_isBarTriggered)
                {
                    _isBarTriggered = false;
                    return;
                }
                Debug.Log($"【TestAudioCallback】 Received: AK_MusicSyncBeat({_beatIndex})");
                //先不使用beatPerBar试试，因为每次AK_MusicSyncBar的时候是会将_beatIndex重置的
                //_beatIndex = (_beatIndex + 1) % beatPerBar;
                _beatIndex++;
                break;
            case AkCallbackType.AK_MusicSyncUserCue:
                var musicInfo = (AkMusicSyncCallbackInfo)inInfo;
                if (musicInfo != null)
                {
                    Debug.Log("【TestAudioCallback】 Received: AK_MusicSyncUserCue, CueName: " + musicInfo.userCueName);
                }
                break;
            default:
                Debug.Log("【TestAudioCallback】 Received: " + inType);
                break;
        }
    }
    #endregion

}



