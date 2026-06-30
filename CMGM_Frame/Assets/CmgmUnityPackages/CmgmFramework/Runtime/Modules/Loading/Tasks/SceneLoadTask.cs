using System;
using System.Threading;
using CMGM.Core;
using CMGM.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CMGM.Loading
{
    /// <summary>场景 Loading 方式：预载资源或激活场景。</summary>
    public enum SceneLoadTaskMode
    {
        /// <summary>预载场景相关 Addressables（不切场景）。</summary>
        Preload,
        /// <summary><see cref="AddressablesResMgr.LoadSceneAsync"/> 激活场景。</summary>
        Load
    }

    /// <summary>
    /// 预载或加载场景（HotRes/Scenes 下场景名，不含路径与后缀）。
    /// </summary>
    public sealed class SceneLoadTask : ILoadTask
    {
        public SceneLoadTask(
            string sceneName,
            SceneLoadTaskMode mode = SceneLoadTaskMode.Preload,
            LoadSceneMode loadSceneMode = LoadSceneMode.Single,
            bool overlayUiCamera = false,
            string displayName = null,
            float weight = 1f)
        {
            if (string.IsNullOrEmpty(sceneName))
                throw new ArgumentException("场景名不能为空。", nameof(sceneName));

            SceneName = sceneName;
            Mode = mode;
            LoadSceneMode = loadSceneMode;
            OverlayUiCamera = overlayUiCamera;
            DisplayName = displayName ?? BuildDisplayName();
            Weight = weight;
        }

        public string SceneName { get; }

        public SceneLoadTaskMode Mode { get; }

        public LoadSceneMode LoadSceneMode { get; }

        /// <summary><see cref="SceneLoadTaskMode.Load"/> 完成后是否叠 UI 摄像机。</summary>
        public bool OverlayUiCamera { get; }

        public string DisplayName { get; }

        public float Weight { get; }

        public async UniTask RunAsync(ILoadProgressReporter reporter, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            reporter.ReportTaskProgress(0f);

            var progress = new Progress<float>(reporter.ReportTaskProgress);

            if (Mode == SceneLoadTaskMode.Preload)
            {
                await AddressablesResMgr.Instance.PreloadAssetsAsync(SceneName, progress);
            }
            else
            {
                await AddressablesResMgr.Instance.LoadSceneAsync(SceneName, LoadSceneMode, progress);

                if (OverlayUiCamera && Camera.main != null)
                    UIManager.Instance.SetUICameraOverlap(Camera.main);
            }

            reporter.ReportTaskProgress(1f);
        }

        private string BuildDisplayName()
        {
            return Mode == SceneLoadTaskMode.Preload
                ? $"预载场景 {SceneName}"
                : $"加载场景 {SceneName}";
        }
    }
}
