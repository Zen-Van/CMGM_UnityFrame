using System.Collections.Generic;
using System.Threading;
using CMGM.Core;
using CMGM.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CMGM.Loading
{
    /// <summary>
    /// 通用加载编排：顺序执行 <see cref="ILoadTask"/> 列表，可选进度 UI。
    /// </summary>
    public sealed class LoadingManager : LazySingleton<LoadingManager>
    {
        public const string LoadingPanelResourcesPath = "UI/Panels/LoadingPanel";

        private LoadingManager() { }

        private LoadingPanel _activePanel;

        /// <summary>
        /// 顺序执行任务列表。#1 总进度 = 等权平均；#4 改为 Weight 加权。
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

            var reporter = new EqualWeightProgressReporter(tasks.Count, _activePanel);

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

            var prefab = ResourcesResMgr.Instance.LoadAsset<GameObject>(LoadingPanelResourcesPath);
            if (prefab == null)
            {
                CmgmLog.fError(
                    $"[LoadingManager] 未找到 Resources/{LoadingPanelResourcesPath}.prefab");
                return;
            }

            var panelObj = Object.Instantiate(prefab, UIManager.Instance.GetLayerNode(layer), false);
            panelObj.name = "LoadingPanel";
            _activePanel = panelObj.GetComponent<LoadingPanel>();
            if (_activePanel == null)
            {
                CmgmLog.fError("[LoadingManager] LoadingPanel.prefab 上缺少 LoadingPanel 组件。");
                Object.Destroy(panelObj);
                return;
            }

            _activePanel.SetProgress(0f);
            _activePanel.SetStatus("加载中…");
            _activePanel.OnShow();
            await UniTask.Yield();
        }

        private void HidePanel()
        {
            if (_activePanel == null)
                return;

            _activePanel.OnHide();
            Object.Destroy(_activePanel.gameObject);
            _activePanel = null;
        }

        /// <summary>#1 验收用演示任务列表。</summary>
        public static IReadOnlyList<ILoadTask> CreateAcceptanceDemoTasks()
        {
            return new ILoadTask[]
            {
                new DelayLoadTask("预载 UI 资源", 400),
                new DelayLoadTask("初始化音频", 500),
                new DelayLoadTask("准备主场景", 600),
                new DelayLoadTask("哥们穿模中…", 700),
            };
        }

        private sealed class EqualWeightProgressReporter : ILoadProgressReporter
        {
            private readonly int _taskCount;
            private readonly LoadingPanel _panel;
            private int _currentTaskIndex;
            private float _currentTaskInternal;

            public EqualWeightProgressReporter(int taskCount, LoadingPanel panel)
            {
                _taskCount = Mathf.Max(1, taskCount);
                _panel = panel;
            }

            public void BeginTask(int taskIndex, string displayName)
            {
                _currentTaskIndex = taskIndex;
                _currentTaskInternal = 0f;
                _panel?.SetStatus(displayName);
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
                _panel?.SetProgress(overall01);
            }

            private float CalculateOverall()
            {
                return (_currentTaskIndex + _currentTaskInternal) / _taskCount;
            }
        }
    }
}
