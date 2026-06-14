using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Video;

public class GameInitializer : MonoBehaviour
{
    public bool SHOW_LOGO = true;

    public VideoPlayer videoPlayer;
    public List<Object> Logos;

    private void Awake()
    {
        if (!SHOW_LOGO) Logos.Clear();

        InitGame();
    }
    

    private bool _gameInitFinished = false;
    /// <summary>
    /// 初始化游戏的方法（包括显示LOGO并跳转主界面）
    /// </summary>
    public void InitGame()
    {
        //显示Logo的同时初始化各游戏系统

        //游戏逻辑层初始化
        UniTask.Void(async () =>
        {
            //将UI包载入内存随时准备使用
            await AddressablesResMgr.Instance.PreloadAssetsAsync("MainScene");
            //初始化UI管理器
            UIManager.Instance.Init();

            //初始化存档管理器，载入存档元数据
            GameArchiveManager.Instance.Init();
            CmgmLog.fPositive($"存档元数据载入完毕，" +
                $"其中共检测到{GameArchiveManager.Instance.ArchiveMeta.dataSet.Count}个存档资料");

            //初始化Lua管理器
            LuaManager.Instance.Init();

            //初始化音频管理器
            WwiseAudioManager.Instance.Init();

            //逻辑层初始化完成
            _gameInitFinished = true;
            CmgmLog.fPositive("游戏逻辑层初始化完成");
        });

        //游戏初始化时的显示层逻辑
        UniTask.Void(async () =>
        {
            //TODO: 加载logo并逐个显示
            for (int i = 0; i < Logos.Count; i++)
            {
                Object logo = Logos[i];
                if (logo is VideoClip)
                {
                    videoPlayer.clip = logo as VideoClip;
                    videoPlayer.time = 0f;

                    videoPlayer.Prepare();
                    videoPlayer.Play();

                    //等视频播完
                    bool videoFinished = false;
                    videoPlayer.loopPointReached += (video) => { videoFinished = true; };
                    await UniTask.WaitUntil(() => videoFinished);

                }
                else if (logo is Texture2D)
                {
                    //如果是图片的话

                }
                else
                {
                    CmgmLog.fNegative($"不支持的logo文件类型：{logo.GetType()}");
                }
            }

            //等待逻辑层初始化完成
            await UniTask.WaitUntil(() => _gameInitFinished);
            //跳转至主界面
            await ScenesManager.Instance.GoToMainScene();

        });
    }
    
}
