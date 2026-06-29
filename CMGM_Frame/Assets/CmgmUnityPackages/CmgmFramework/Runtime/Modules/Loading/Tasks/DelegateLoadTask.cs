using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace CMGM.Loading
{
    /// <summary>
    /// 将已有 async 逻辑包装为 <see cref="ILoadTask"/>（#5 过渡包 <c>GameBootstrap</c> 等）。
    /// </summary>
    public sealed class DelegateLoadTask : ILoadTask
    {
        private readonly Func<CancellationToken, UniTask> _action;

        public DelegateLoadTask(
            string displayName,
            Func<CancellationToken, UniTask> action,
            float weight = 1f)
        {
            DisplayName = displayName;
            Weight = weight;
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public DelegateLoadTask(string displayName, Func<UniTask> action, float weight = 1f)
            : this(displayName, _ => action(), weight)
        {
        }

        public string DisplayName { get; }

        public float Weight { get; }

        public async UniTask RunAsync(ILoadProgressReporter reporter, CancellationToken cancellationToken)
        {
            reporter.ReportTaskProgress(0f);
            await _action(cancellationToken);
            reporter.ReportTaskProgress(1f);
        }
    }
}
