
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


    #region 基础接口
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

    /// <summary> 播放音频事件 </summary>
    /// <param name="wwiseEvent">Wwise事件</param>
    /// <param name="emitter">音源物体（可选）</param>
    public void PlayWwiseEvent(AK.Wwise.Event wwiseEvent, GameObject emitter = null)
    {
        if (wwiseEvent == null) return;
        wwiseEvent.Post(emitter == null ? gameObject : emitter);
    }
    
    /// <summary> 播放带回调的音频事件 </summary>
    /// <param name="wwiseEvent">Wwise事件</param>
    /// <param name="emitter">音源物体</param>
    /// <param name="callbackFlags">回调类型</param>
    /// <param name="callbackFunc">回调函数</param>
    /// <returns></returns>
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
        uint playingId = wwiseEvent.Post(emitter, (uint)callbackFlags, CallbackFunc);

        return playingId;
    }
    #endregion

    #region 音乐通用回调接口（也可作为非通用回调的案例）
    /// <summary> 设置节拍参数 </summary>
    public static bool _isBarTriggered = false;
    public static int _beatIndex = 0;

    public static void CallbackFunc(object inCookie, AkCallbackType inType, AkCallbackInfo inInfo)
    {
        switch (inType)
        {
            case AkCallbackType.AK_MusicSyncEntry:
                Debug.Log("【TestAudioCallback】 Received: AK_MusicSyncEntry");
                break;
            case AkCallbackType.AK_MusicSyncExit:
                Debug.Log("【TestAudioCallback】 Received: AK_MusicSyncExit");
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
            case AkCallbackType.AK_EnableGetMusicPlayPosition:
                // 这个回调类型通常用于获取音乐播放位置
                break;
            default:
                Debug.Log("【TestAudioCallback】 Received: " + inType);
                break;
        }
    }
    #endregion
}