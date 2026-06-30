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
                LoadingRunOptions.WithProgress);
        }

        private IReadOnlyList<ILoadTask> CreateTasks()
        {
            return new ILoadTask[]
            {
                new TableLoadTask<RoleInfo>(),
                new EnterGameplayLoadTask(),
            };
        }
    }
}
