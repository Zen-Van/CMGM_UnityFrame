using System.Collections.Generic;
using CMGM.Core;
using CMGM.GameFlow;
using CMGM.Loading;
using Cysharp.Threading.Tasks;

namespace CMGM.Workspace.GameFlowState
{
    /// <summary>
    /// 宏观态 WorldMap（空壳）：同态换区域用 <see cref="TravelTo"/>，不 SwitchTo。
    /// </summary>
    public sealed class WorldMapState : GameFlowStateBase
    {
        public WorldMapState() : base("WorldMap") { }

        public override UniTask EnterAsync()
        {
            return TravelTo("World_Default");
        }

        public async UniTask TravelTo(string regionId)
        {
            CmgmLog.fNormal($"[WorldMap] TravelTo → {regionId}");
            await LoadingManager.Instance.RunAsync(
                CreateTravelTasks(regionId),
                new LoadingRunOptions { ShowProgress = true });
        }

        private IReadOnlyList<ILoadTask> CreateTravelTasks(string regionId)
        {
            return new ILoadTask[]
            {
                new DelegateLoadTask(
                    $"切换区域 {regionId}（占位）",
                    _ => UniTask.CompletedTask,
                    weight: 1f),
            };
        }
    }
}
