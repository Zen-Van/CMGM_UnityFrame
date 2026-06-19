
using CMGM.Core;
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

    /// <summary>当前由 PlayCommonBgm 播放的 BGM 的 PlayingId。</summary>
    public uint CurBgmPlayingId { get; private set; } = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;

    /// <summary>
    /// 播放通用 BGM（纯播放，不含节拍同步）。
    /// 需要节拍同步的音游场景请用 MusicGame 的 <c>MusicSyncTool.PlayBgmWithBeatSync</c>。
    /// </summary>
    public uint PlayCommonBgm(string eventName, GameObject emitter = null)
    {
        CurBgmPlayingId = PlayWwiseEvent(eventName, emitter);
        return CurBgmPlayingId;
    }
    public void StopCommonBgm()
    {
        AkUnitySoundEngine.StopPlayingID(CurBgmPlayingId);
        CurBgmPlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
    }
    #endregion

    #region 全局音量调整接口
    public void SetMasterVolume(float value) => AkUnitySoundEngine.SetRTPCValue(masterVolumeRtpcName, value);
    public void SetMusicVolume(float value) => AkUnitySoundEngine.SetRTPCValue(musicVolumeRtpcName, value);
    public void SetSfxVolume(float value)=>AkUnitySoundEngine.SetRTPCValue(sfxVolumeRtpcName,value);
    public void SetVoiceVolume(float value) => AkUnitySoundEngine.SetRTPCValue(voiceVolumeRtpcName, value);
    #endregion
}