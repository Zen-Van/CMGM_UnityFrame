using CMGM.UI;

namespace CMGM.Loading
{
    /// <summary>
    /// <see cref="LoadingManager.RunAsync"/> 运行参数。
    /// </summary>
    public sealed class LoadingRunOptions
    {
        /// <summary>是否显示进度面板（#1 默认 true）。</summary>
        public bool ShowProgress { get; set; } = true;

        /// <summary>进度面板挂载 UI 层（默认 System，盖住普通面板）。</summary>
        public E_UILayer Layer { get; set; } = E_UILayer.System;
    }
}
