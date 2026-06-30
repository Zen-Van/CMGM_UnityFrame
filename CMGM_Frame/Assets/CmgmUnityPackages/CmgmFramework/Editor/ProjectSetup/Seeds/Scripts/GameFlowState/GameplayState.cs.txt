using System.Collections.Generic;
using CMGM.GameFlow;
using CMGM.Loading;
using CMGM.Workspace.LoadTasks;
using Cysharp.Threading.Tasks;

namespace CMGM.Workspace.GameFlowState
{
    /// <summary>
    /// 宏观态 Gameplay（#8 样板）：主菜单 → 进游戏。
    /// </summary>
    public sealed class GameplayState : GameFlowStateBase
    {
        public GameplayState() : base("Gameplay") { }

        public override async UniTask EnterAsync()
        {
            await LoadingManager.Instance.RunAsync(
                CreateTasks(),
                new LoadingRunOptions { ShowProgress = true });
        }

        private IReadOnlyList<ILoadTask> CreateTasks()
        {
            return new ILoadTask[]
            {
                new TableLoadTask<RoleInfo>(),
                // 表多了继续加 TableLoadTask<XXX>()，不必新建 Task 类
                // TODO #16：废止 GameBootstrap 后，进场景 / Bank 等用具名 LoadTask
                new DelegateLoadTask(
                    "进游戏（过渡）",
                    _ => GameBootstrap.EnterGameplayAsync(),
                    weight: 5f),
            };
        }
    }
}
