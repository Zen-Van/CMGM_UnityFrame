using System.Collections.Generic;
using System.Threading;
using CMGM.Core;
using CMGM.UI;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CMGM.Loading
{
    /// <summary>
    /// 通用加载编排：顺序执行 <see cref="ILoadTask"/> 列表，可选进度 UI。
    /// </summary>
    public sealed class LoadingManager : LazySingleton<LoadingManager>
    {
        public const string LoadingPanelName = "LoadingPanel";

        /// <summary>LoadingPanel Prefab 控件名约定（与业务层 Prefab 一致）。</summary>
        private const string ProgressSliderControl = "sldProgress";
        private const string StatusTextControl = "txtStatus";

        private LoadingManager() { }

        private BasePanel _activePanel;

        /// <summary>
        /// 顺序执行任务列表；总进度 = Σ(已完成 Weight) + 当前 Weight × 任务内进度。
        /// </summary>
        public async UniTask RunAsync(
            IReadOnlyList<ILoadTask> tasks,
            LoadingRunOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (tasks == null || tasks.Count == 0)
                return;

            if (!UIManager.IsReady)
            {
                CmgmLog.fError("[LoadingManager] UIManager 尚未 InitAsync，无法显示 Loading UI。");
                return;
            }

            options ??= new LoadingRunOptions();

            if (options.ShowProgress)
                await ShowPanelAsync(options.Layer);

            var reporter = new WeightedProgressReporter(tasks, _activePanel);

            try
            {
                for (var i = 0; i < tasks.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var task = tasks[i];
                    reporter.BeginTask(i, task.DisplayName);
                    await task.RunAsync(reporter, cancellationToken);
                    reporter.CompleteTask(i);
                }

                reporter.ReportOverall(1f);
            }
            finally
            {
                if (options.ShowProgress)
                    HidePanel();
            }
        }

        private async UniTask ShowPanelAsync(E_UILayer layer)
        {
            if (_activePanel != null)
                return;

            _activePanel = await UIManager.Instance.ShowPanel(LoadingPanelName, layer);
            if (_activePanel == null)
            {
                CmgmLog.fError(
                    $"[LoadingManager] 未找到 {LoadingPanelName}（HotRes/UI/Panels/{LoadingPanelName}.prefab）");
                return;
            }

            ApplyLoadingProgress(_activePanel, 0f);
            ApplyLoadingStatus(_activePanel, "加载中…");
        }

        private void HidePanel()
        {
            if (_activePanel == null)
                return;

            UIManager.Instance.HidePanel(LoadingPanelName, isDestroy: true);
            _activePanel = null;
        }

        private static void ApplyLoadingProgress(BasePanel panel, float overall01)
        {
            var slider = panel?.GetControl<Slider>(ProgressSliderControl);
            if (slider != null)
                slider.value = Mathf.Clamp01(overall01);
        }

        private static void ApplyLoadingStatus(BasePanel panel, string text)
        {
            var label = panel?.GetControl<TMP_Text>(StatusTextControl);
            if (label != null)
                label.text = text;
        }

        private sealed class WeightedProgressReporter : ILoadProgressReporter
        {
            private readonly IReadOnlyList<ILoadTask> _tasks;
            private readonly float[] _weights;
            private readonly float _totalWeight;
            private readonly BasePanel _panel;
            private int _currentTaskIndex;
            private float _currentTaskInternal;

            public WeightedProgressReporter(IReadOnlyList<ILoadTask> tasks, BasePanel panel)
            {
                _tasks = tasks;
                _panel = panel;
                _weights = new float[tasks.Count];
                _totalWeight = 0f;

                for (var i = 0; i < tasks.Count; i++)
                {
                    var w = tasks[i].Weight;
                    if (w <= 0f)
                        w = 1f;
                    _weights[i] = w;
                    _totalWeight += w;
                }

                if (_totalWeight <= 0f)
                    _totalWeight = tasks.Count;
            }

            public void BeginTask(int taskIndex, string displayName)
            {
                _currentTaskIndex = taskIndex;
                _currentTaskInternal = 0f;
                ApplyLoadingStatus(_panel, displayName);
                ReportOverall(CalculateOverall());
            }

            public void CompleteTask(int taskIndex)
            {
                _currentTaskIndex = taskIndex;
                _currentTaskInternal = 1f;
                ReportOverall(CalculateOverall());
            }

            public void ReportTaskProgress(float taskInternal01)
            {
                _currentTaskInternal = Mathf.Clamp01(taskInternal01);
                ReportOverall(CalculateOverall());
            }

            public void ReportOverall(float overall01)
            {
                ApplyLoadingProgress(_panel, overall01);
            }

            private float CalculateOverall()
            {
                var completed = 0f;
                for (var i = 0; i < _currentTaskIndex; i++)
                    completed += _weights[i];

                var currentContribution = _weights[_currentTaskIndex] * _currentTaskInternal;
                return (completed + currentContribution) / _totalWeight;
            }
        }
    }
}
