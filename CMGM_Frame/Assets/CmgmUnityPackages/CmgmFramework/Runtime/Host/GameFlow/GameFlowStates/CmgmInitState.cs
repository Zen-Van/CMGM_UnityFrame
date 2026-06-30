using System.Collections.Generic;
using CMGM.Core;
using CMGM.Data;
using CMGM.Loading;
using CMGM.UI;
using Cysharp.Threading.Tasks;

namespace CMGM.GameFlow
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
                LoadingRunOptions.Silent);
            CmgmLog.fPositive("游戏逻辑层初始化完成");
        }

        private IReadOnlyList<ILoadTask> CreateTasks()
        {
            var mainScene = CmgmFrameSettings.Instance.MAIN_SCENE_NAME;
            return new ILoadTask[]
            {
                new SceneLoadTask(mainScene, SceneLoadTaskMode.Preload, weight: 2f),
                new ManagerInitLoadTask<ArchiveManager>("初始化存档", weight: 2f),
                new ManagerInitLoadTask<LuaManager>("初始化 Lua", weight: 3f),
                new WwiseInitLoadTask(weight: 1f),
            };
        }
    }
}
