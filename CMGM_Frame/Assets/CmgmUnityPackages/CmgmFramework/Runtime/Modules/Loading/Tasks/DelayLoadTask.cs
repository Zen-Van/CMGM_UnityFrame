using System.Threading;
using Cysharp.Threading.Tasks;

namespace CMGM.Loading
{
    /// <summary>
    /// #1 演示 / 测试用：按毫秒延时并分段上报进度。
    /// </summary>
    public sealed class DelayLoadTask : ILoadTask
    {
        private readonly int _delayMs;

        public DelayLoadTask(string displayName, int delayMs, float weight = 1f)
        {
            DisplayName = displayName;
            _delayMs = delayMs;
            Weight = weight;
        }

        public string DisplayName { get; }

        public float Weight { get; }

        public async UniTask RunAsync(ILoadProgressReporter reporter, CancellationToken cancellationToken)
        {
            if (_delayMs <= 0)
            {
                reporter.ReportTaskProgress(1f);
                return;
            }

            const int steps = 10;
            var stepDelay = _delayMs / steps;
            for (var i = 1; i <= steps; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (stepDelay > 0)
                    await UniTask.Delay(stepDelay, cancellationToken: cancellationToken);
                reporter.ReportTaskProgress(i / (float)steps);
            }
        }
    }
}
