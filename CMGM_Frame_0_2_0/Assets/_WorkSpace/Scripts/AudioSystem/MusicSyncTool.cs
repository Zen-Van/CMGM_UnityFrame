using UnityEngine;

public static class MusicSyncTool
{
    #region 音乐播放数据
    public static uint curGameBgmPlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
    public static uint curGameBgmEventId = 0;

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
                    $"当前播放的音乐的PlayingId为：{curGameBgmPlayingId}\n"+
                    $"当前播放的音乐的EventId为：{AkUnitySoundEngine.GetEventIDFromPlayingID(curGameBgmPlayingId)}");

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
            default:
                Debug.Log("【TestAudioCallback】 Received: " + inType);
                break;
        }
    }
    #endregion

}
