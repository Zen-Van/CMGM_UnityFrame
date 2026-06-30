using System.Threading;
using CMGM.Audio;
using Cysharp.Threading.Tasks;

namespace CMGM.Loading
{
    /// <summary>
    /// 唤醒 InitScene 上的 <see cref="WwiseAudioManager"/>（WwiseGlobal 须在场景中已挂载）。
    /// </summary>
    public sealed class WwiseInitLoadTask : ILoadTask
    {
        public WwiseInitLoadTask(string displayName = "初始化 Wwise", float weight = 1f)
        {
            DisplayName = displayName;
            Weight = weight;
        }

        public string DisplayName { get; }

        public float Weight { get; }

        public UniTask RunAsync(ILoadProgressReporter reporter, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            reporter.ReportTaskProgress(0f);

            _ = WwiseAudioManager.Instance;

            reporter.ReportTaskProgress(1f);
            return UniTask.CompletedTask;
        }
    }
}
