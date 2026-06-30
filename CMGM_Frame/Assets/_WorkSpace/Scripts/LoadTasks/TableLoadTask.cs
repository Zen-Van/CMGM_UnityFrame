using System.Threading;
using CMGM.Data;
using CMGM.Loading;
using Cysharp.Threading.Tasks;

namespace CMGM.Workspace.LoadTasks
{
    /// <summary>
    /// 预加载一张配表（泛型，对标框架 <see cref="ManagerInitLoadTask{T}"/>）。
    /// </summary>
    public sealed class TableLoadTask<T> : ILoadTask
    {
        public TableLoadTask(string displayName = null, float weight = 1f)
        {
            DisplayName = displayName ?? $"加载配表 {typeof(T).Name}";
            Weight = weight;
        }

        public string DisplayName { get; }

        public float Weight { get; }

        public UniTask RunAsync(ILoadProgressReporter reporter, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            reporter.ReportTaskProgress(0f);

            ConfigTableManager.Instance.LoadTable<T>();

            reporter.ReportTaskProgress(1f);
            return UniTask.CompletedTask;
        }
    }
}
