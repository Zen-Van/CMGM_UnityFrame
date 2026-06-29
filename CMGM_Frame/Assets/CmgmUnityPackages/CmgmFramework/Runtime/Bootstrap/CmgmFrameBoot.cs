using System.Collections.Generic;
using CMGM.Audio;
using CMGM.Core;
using CMGM.Data;
using CMGM.GameFlow;
using CMGM.Loading;
using CMGM.Scene;
using CMGM.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace CMGM.Bootstrap
{
    /// <summary>
    /// InitScene 框架组合根：Logo 展示 + BootSingleton InitAsync 链 + 进主界面。
    /// </summary>
    public class CmgmFrameBoot : MonoBehaviour
    {
        public bool SHOW_LOGO = true;

        [Header("Loading 1.1 / 1.2 验收")]
        [Tooltip("Init 完成后、进主界面前跑演示 Loading（加权进度 + ManagerInitLoadTask；验收通过后可关）")]
        public bool runLoadingAcceptanceDemo = true;

        [Header("GameFlow 1.2 验收")]
        [Tooltip("进主界面前跑 SwitchTo / Push / Pop 日志演示（#3 验收通过后可关）")]
        public bool runGameFlowAcceptanceDemo = true;

        [Header("Logo 展示")]
        [Tooltip("InitScene 上用于 Logo 视频/图片的全屏 Canvas（sortingOrder 较高，Loading 前须隐藏）")]
        public Canvas logoPresentationCanvas;

        public VideoPlayer videoPlayer;
        public List<Object> Logos;

        private void Awake()
        {
            if (!SHOW_LOGO) Logos.Clear();

            InitGame();
        }

        public static bool GameInitFinished => _gameInitFinished;
        private static bool _gameInitFinished = false;
        
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
                await AddressablesResMgr.Instance.PreloadAssetsAsync(CmgmFrameSettings.Instance.MAIN_SCENE_NAME);
                await UIManager.InitAsync();
                await ArchiveManager.InitAsync();
                CmgmLog.fPositive($"存档元数据载入完毕，" +
                    $"其中共检测到{ArchiveManager.Instance.ArchiveMeta.dataSet.Count}个存档资料");
                await LuaManager.InitAsync();

                // 音频：维持现状，Audio 支线再定 Boot 策略
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

                //logo和逻辑层都完成了再淡出隐藏 Logo
                await HideLogoPresentationAsync();

                if (runLoadingAcceptanceDemo)
                {
                    await LoadingManager.Instance.RunAsync(
                        CreateLoadingAcceptanceDemoTasks(),
                        new LoadingRunOptions { ShowProgress = true });
                }

                if (runGameFlowAcceptanceDemo)
                    RunGameFlowAcceptanceDemo();

                //跳转至主界面
                await ScenesManager.Instance.GoToMainScene();

            });
        }

        private async UniTask HideLogoPresentationAsync()
        {
            videoPlayer?.Stop();

            var logoRoot = logoPresentationCanvas != null
                ? logoPresentationCanvas.gameObject
                : GameObject.Find("VedioImage")?.GetComponentInParent<Canvas>()?.gameObject;
            if (logoRoot == null)
                return;

            if (!logoRoot.TryGetComponent(out CanvasGroup cg))
                cg = logoRoot.AddComponent<CanvasGroup>();

            for (float t = 0.5f; t > 0f; t -= Time.deltaTime, cg.alpha = t / 0.5f)
                await UniTask.Yield();

            logoRoot.SetActive(false);
        }

        private static IReadOnlyList<ILoadTask> CreateLoadingAcceptanceDemoTasks()
        {
            // #4 验收用临时清单（非 Profile SO）；#6 后 Startup/EnterGameplay 迁入 LoadingProfile
            return new ILoadTask[]
            {
                new ManagerInitLoadTask<ArchiveManager>("存档 Manager（Task）", weight: 2f),
                new DelayLoadTask("预载 UI 资源", 400, weight: 1f),
                new DelayLoadTask("初始化音频", 500, weight: 2f),
                new DelayLoadTask("准备主场景", 800, weight: 8f),
                new DelayLoadTask("哥们穿模中...", 300, weight: 1f),
            };
        }

        private static void RunGameFlowAcceptanceDemo()
        {
            var flow = GameFlowMachine.Instance;
            var mainMenu = new LogGameFlowState("MainMenu");
            var gameplay = new LogGameFlowState("Gameplay");
            var pause = new LogGameFlowState("Pause");

            flow.SwitchTo(mainMenu);
            flow.Push(pause);
            flow.Pop();
            flow.SwitchTo(gameplay);
        }

    }
}
