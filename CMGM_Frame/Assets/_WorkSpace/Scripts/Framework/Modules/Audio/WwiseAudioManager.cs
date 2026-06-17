
using CMGM.Core;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

/// <summary>
/// Wwise音频管理器（须手动绑在初始化场景的WwiseGlobal上）
/// </summary>
public class WwiseAudioManager : SingletonAutoMono<WwiseAudioManager>
{
    override protected void Awake()
    {
        base.Awake();

        // 开始自动加载主bank
        if (loadBankOnAwake)
        {
            LoadBank(mainBankName);
        }
    }

    [Header("Bus Volume")]
    private string masterVolumeRtpcName = "MasterVolume";
    private string musicVolumeRtpcName = "MusicVolume";
    private string sfxVolumeRtpcName = "SfxVolume";
    private string voiceVolumeRtpcName = "VoiceVolume";

    [Header("Wwise Banks")]
    private string mainBankName = "DefaultBank";
    private bool loadBankOnAwake = true;

    #region 方便调试的音频数据
    //应当写到节拍输入的脚本中（输入控制器中设置窗口而非音频系统中设置窗口）
    //最好设置成根据歌曲的bpm而适应变化的，现在这个数值在bpm超过180的时候可能不适用
    [BoxGroup("节拍判定窗口（单位：毫秒）")] public int goodWindow = 150;
    [BoxGroup("节拍判定窗口（单位：毫秒）")] public int greatWindow = 100;
    [BoxGroup("节拍判定窗口（单位：毫秒）")] public int perfectWindow = 50;
    #endregion

    #region Bank管理接口
    public void LoadBank(AK.Wwise.Bank bank)
    {
        if (bank == null) return;
        bank.Load();
    }
    public uint LoadBank(string bankName)
    {
        uint bankId = 0;
        AkUnitySoundEngine.LoadBank(bankName, out bankId);
        return bankId;
    }
    public void UnloadBank(AK.Wwise.Bank bank)
    {
        if (bank == null) return;
        bank.Unload();
    }
    public AKRESULT UnloadBank(string bankName)
    {
        return AkUnitySoundEngine.UnloadBank(bankName, IntPtr.Zero);
    }
    #endregion

    #region 基础播放接口
    //以下播放接口仅播放Wwise事件，不会将播放事件纳入音乐/音效/语音的分类管理
    //若想要播放音乐音效请使用PlayCommonSfx/PlayCommonBgm/PlayCommonVo
    public uint PlayWwiseEvent(AK.Wwise.Event wwiseEvent, GameObject emitter = null)
    {
        if (wwiseEvent == null) return AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
        return wwiseEvent.Post(emitter == null ? gameObject : emitter);
    }
    public uint PlayWwiseEvent(string eventName, GameObject emitter = null)
    {
        return AkUnitySoundEngine.PostEvent(eventName, emitter == null ? gameObject : emitter);
    }


    public uint PlayWwiseEventWithCallback(AK.Wwise.Event wwiseEvent, GameObject emitter,
        AkCallbackType callbackFlags, AkCallbackManager.EventCallback callbackFunc)
    {
        if (wwiseEvent == null)
        {
            return AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
        }
        if (emitter == null)
        {
            emitter = gameObject;
        }
        uint playingId = wwiseEvent.Post(emitter, (uint)callbackFlags, callbackFunc);

        return playingId;
    }
    public uint PlayWwiseEventWithCallback(string eventName, GameObject emitter,
        AkCallbackType callbackFlags, AkCallbackManager.EventCallback callbackFunc)
    {
        if (emitter == null)
        {
            emitter = gameObject;
        }
        return AkUnitySoundEngine.PostEvent(eventName, emitter, (uint)callbackFlags, callbackFunc, null);
    }
    #endregion

    #region 进一步封装播放接口
    public uint PlayCommonSfx(string eventName, GameObject emitter = null)
    {
        return PlayWwiseEvent(eventName, emitter);
    }
    public uint PlayCommonVo(string eventName,GameObject emitter = null)
    {
        return PlayWwiseEvent(eventName, emitter);
    }
    public uint PlayCommonBgm(string eventName, string bgmNameForEvtList = null, AkCallbackManager.EventCallback callbackFunc = null)
    {
        MusicSyncTool.curBgmEventId = AkUnitySoundEngine.GetIDFromString(eventName);

        MusicSyncTool.curBgmPlayingId = PlayWwiseEventWithCallback(eventName, gameObject,
            AkCallbackType.AK_EnableGetMusicPlayPosition | AkCallbackType.AK_EnableGetSourcePlayPosition | AkCallbackType.AK_MusicSyncAll,
            callbackFunc == null ? MusicSyncTool.MusicEventDefaultCallbackFunc : callbackFunc);
        
        //是否开启音乐节拍计算
        if(bgmNameForEvtList != null)
            MusicSyncTool.ActiveMusicBeatSync(Consts.Paths.RhythmMap_Path + $"/{bgmNameForEvtList}/{bgmNameForEvtList}_BeatEvtList.json");
        else
            MusicSyncTool.DisableMusicBeatSync();

        return MusicSyncTool.curBgmPlayingId;
    }
    public void StopCommonBgm(uint playingId)
    {
        AkUnitySoundEngine.StopPlayingID(MusicSyncTool.curBgmPlayingId);
        MusicSyncTool.DisableMusicBeatSync();
    }
    #endregion

    #region 全局音量调整接口
    public void SetMasterVolume(float value) => AkUnitySoundEngine.SetRTPCValue(masterVolumeRtpcName, value);
    public void SetMusicVolume(float value) => AkUnitySoundEngine.SetRTPCValue(musicVolumeRtpcName, value);
    public void SetSfxVolume(float value)=>AkUnitySoundEngine.SetRTPCValue(sfxVolumeRtpcName,value);
    public void SetVoiceVolume(float value) => AkUnitySoundEngine.SetRTPCValue(voiceVolumeRtpcName, value);
    #endregion
}