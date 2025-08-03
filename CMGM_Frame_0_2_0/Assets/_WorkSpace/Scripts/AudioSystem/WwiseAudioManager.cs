
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

/// <summary>
/// Wwise音频管理器（须手动绑在初始化场景的WwiseGlobal上）
/// </summary>
public class WwiseAudioManager : SingletonMono<WwiseAudioManager>
{
    override protected void Awake()
    {
        base.Awake();

        // 开始自动加载主bank
        if (loadBankOnAwake && mainBank != null)
        {
            mainBank.Load();
        }
    }

    [Header("Wwise Banks")]
    public AK.Wwise.Bank mainBank;
    [FormerlySerializedAs("LoadBankOnAwake")] public bool loadBankOnAwake = true;


    [Header("Wwise Game Parameters")]
    public AK.Wwise.RTPC masterVolume;
    public AK.Wwise.RTPC musicVolume;
    public AK.Wwise.RTPC sfxVolume;
    public AK.Wwise.RTPC voiceVolume;


    #region 基础公共接口
    public void LoadBank(AK.Wwise.Bank bank)
    {
        if (bank == null) return;
        bank.Load();
    }
    public void UnloadBank(AK.Wwise.Bank bank)
    {
        if (bank == null) return;
        bank.Unload();
    }


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

    #region 进一步封装公共接口
    public uint PlayCommonSfx(string eventName, GameObject emitter = null)
    {
        return PlayWwiseEvent(eventName, emitter);
    }
    public uint PlayCommonVo(string eventName,GameObject emitter = null)
    {
        return PlayWwiseEvent(eventName, emitter);
    }
    public uint PlayCommonBgm(string eventName, AkCallbackManager.EventCallback callbackFunc = null)
    {
        MusicSyncTool.curGameBgmEventId = AkUnitySoundEngine.GetIDFromString(eventName);

        MusicSyncTool.curGameBgmPlayingId = PlayWwiseEventWithCallback(eventName, gameObject,
            AkCallbackType.AK_EnableGetMusicPlayPosition | AkCallbackType.AK_EnableGetSourcePlayPosition | AkCallbackType.AK_MusicSyncAll,
            callbackFunc == null ? MusicSyncTool.MusicEventDefaultCallbackFunc : callbackFunc);
        return MusicSyncTool.curGameBgmPlayingId;
    }
    #endregion

}