using System.Threading;
using CMGM.Core;
using CMGM.Loading;
using Cysharp.Threading.Tasks;

namespace CMGM.Workspace.LoadTasks
{
    /// <summary>
    /// 进游戏：关卡场景、Addressables、Wwise Bank 等（配表用 <see cref="TableLoadTask{T}"/>）。
    /// </summary>
    public sealed class EnterGameplayLoadTask : ILoadTask
    {
        public EnterGameplayLoadTask(string displayName = "进游戏", float weight = 5f)
        {
            DisplayName = displayName;
            Weight = weight;
        }

        public string DisplayName { get; }

        public float Weight { get; }

        public async UniTask RunAsync(ILoadProgressReporter reporter, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            reporter.ReportTaskProgress(0f);

            // TODO：预载关卡场景 / Addressables、Wwise Bank 等
            CmgmLog.fPositive("进游戏内容加载完成");

            reporter.ReportTaskProgress(1f);
            await UniTask.CompletedTask;
        }
    }
}
