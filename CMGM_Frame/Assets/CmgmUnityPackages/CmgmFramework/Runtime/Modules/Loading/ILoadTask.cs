using System.Threading;
using Cysharp.Threading.Tasks;

namespace CMGM.Loading
{
    /// <summary>
    /// 单条可编排加载任务（#1 基础契约；#4 起使用 <see cref="Weight"/> 加权总进度）。
    /// </summary>
    public interface ILoadTask
    {
        string DisplayName { get; }

        /// <summary>任务权重；总进度按 Weight 加权（#4）。</summary>
        float Weight => 1f;

        UniTask RunAsync(ILoadProgressReporter reporter, CancellationToken cancellationToken);
    }

    /// <summary>
    /// 当前任务内部进度上报（0~1）。总进度由 <see cref="LoadingManager"/> 汇总。
    /// </summary>
    public interface ILoadProgressReporter
    {
        void ReportTaskProgress(float taskInternal01);
    }
}
