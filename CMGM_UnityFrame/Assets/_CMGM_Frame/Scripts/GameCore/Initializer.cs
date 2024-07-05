using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Initializer : MonoBehaviour
{
    public bool SHOW_LOGO = true;

    public VideoPlayer videoPlayer;
    public List<Object> Logos;

    private void Awake()
    {
        if (!SHOW_LOGO) Logos.Clear();

        InitGame();
    }

    /// <summary>
    /// 初始化游戏的方法（包括显示LOGO并跳转主界面）
    /// </summary>
    public void InitGame()
    {
        //显示Logo的同时初始化各游戏系统

        //游戏逻辑层初始化
        UniTask.Void(async () =>
        {
            await ResManager.Instance.LoadABAsync("UI");//将UI包载入内存随时准备使用
            UIManager.Instance.Init();//初始化UI管理器

            //加载所有场景AB包
            await ResManager.Instance.LoadABAsync("scenes");

            //初始化存档管理器，载入存档元数据
            GameArchiveManager.Instance.Init();
            CmgmLog.FrameLogPositive($"存档元数据载入完毕，" +
                $"其中共检测到{GameArchiveManager.Instance.ArchiveMeta.dataSet.Count}个存档资料");

            //逻辑层初始化完成
            GameSystem.GameInitFinished = true;
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
                    CmgmLog.FrameLog_Negative($"不支持的logo文件类型：{logo.GetType()}");
                }
            }

            //等待逻辑层初始化完成
            await UniTask.WaitUntil(() => GameSystem.GameInitFinished);
            //跳转至主界面
            await GameSystem.GoToMainUI();
            //把beforeGame包卸载了
            ResManager.Instance.UnLoad_AssetBundle("before_game", true);
        });
    }
}
