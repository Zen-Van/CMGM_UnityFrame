using System.Collections.Generic;
using CMGM.Audio;
using CMGM.Core;
using CMGM.Data;
using CMGM.GameFlow;
using CMGM.Loading;
using CMGM.UI;
using Cysharp.Threading.Tasks;

namespace CMGM.Bootstrap.GameFlow
{
    /// <summary>
    /// 宏观态 <c>CmgmInit</c>（#7）：UIManager + 静默启动 Loading，可与 Logo 并行；CreateTasks 与 RunAsync 同址（§6.5f）。
    /// </summary>
    public sealed class CmgmInitState : GameFlowStateBase
    {
        public CmgmInitState() : base("CmgmInit") { }

        public override async UniTask EnterAsync()
        {
            // LoadingManager.RunAsync 要求 UIManager 已 Ready（含 ShowProgress=false）
            await UIManager.InitAsync();
            await LoadingManager.Instance.RunAsync(
                CreateTasks(),
                new LoadingRunOptions { ShowProgress = false });
            CmgmLog.fPositive("游戏逻辑层初始化完成");
        }

        /// <summary>启动框架任务列表（静默、无进度条；#10 可拆成具名 <see cref="ILoadTask"/> 类）。</summary>
        private IReadOnlyList<ILoadTask> CreateTasks()
        {
            var mainScene = CmgmFrameSettings.Instance.MAIN_SCENE_NAME;
            return new ILoadTask[]
            {
                new DelegateLoadTask(
                    "预载主场景",
                    _ => AddressablesResMgr.Instance.PreloadAssetsAsync(mainScene),
                    weight: 2f),
                new ManagerInitLoadTask<ArchiveManager>("初始化存档", weight: 2f),
                new ManagerInitLoadTask<LuaManager>("初始化 Lua", weight: 3f),
                new DelegateLoadTask(
                    "初始化 Wwise 壳",
                    _ =>
                    {
                        // 音频：维持现状，Audio 支线再定 Boot 策略
                        WwiseAudioManager.Instance.Init();
                        return UniTask.CompletedTask;
                    },
                    weight: 1f),
            };
        }
    }
}
