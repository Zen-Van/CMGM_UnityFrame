using System.Threading;
using CMGM.Core;
using Cysharp.Threading.Tasks;

namespace CMGM.Loading
{
    /// <summary>
    /// 包装 <see cref="BootSingleton{T}.InitAsync"/>，将 Manager 初始化纳入 Loading 编排（#4）。
    /// </summary>
    public sealed class ManagerInitLoadTask<T> : ILoadTask where T : BootSingleton<T>
    {
        public ManagerInitLoadTask(string displayName, float weight = 1f)
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
            await BootSingleton<T>.InitAsync();
            reporter.ReportTaskProgress(1f);
        }
    }
}
