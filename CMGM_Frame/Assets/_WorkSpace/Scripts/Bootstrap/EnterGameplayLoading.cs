using System.Collections.Generic;
using CMGM.Loading;

namespace CMGM.Workspace
{
    /// <summary>
    /// 主界面 → 进游戏 Loading 任务清单（#5 过渡；#6 迁入 <c>EnterGameplay</c> Profile SO，#13 废止 <see cref="GameBootstrap"/>）。
    /// </summary>
    public static class EnterGameplayLoading
    {
        /// <summary>临时 EnterGameplay 任务列表；当前以 <see cref="DelegateLoadTask"/> 包装 <see cref="GameBootstrap.EnterGameplayAsync"/>。</summary>
        public static IReadOnlyList<ILoadTask> CreateTasks()
        {
            return new ILoadTask[]
            {
                new DelegateLoadTask(
                    "进游戏",
                    _ => GameBootstrap.EnterGameplayAsync(),
                    weight: 5f),
            };
        }
    }
}
