using CMGM.Core;
using CMGM.Data;
using Cysharp.Threading.Tasks;

namespace CMGM.Workspace
{
    /// <summary>
    /// 业务层「进游戏」加载逻辑（#5 由 <see cref="EnterGameplayLoading"/> + <see cref="DelegateLoadTask"/> 接入 Loading 进度条）。
    /// <para>#13 废止本类，清单迁入 <c>EnterGameplay</c> Profile SO。</para>
    /// </summary>
    public static class GameBootstrap
    {
        /// <summary>
        /// 进游戏所需内容（配表、关卡资源、Bank 等）。由 Loading 任务调用，MainPanel 不直调。
        /// </summary>
        public static async UniTask EnterGameplayAsync()
        {
            ConfigTableManager.Instance.LoadTable<RoleInfo>();
            // TODO：预载关卡场景 / Addressables、Wwise Bank 等
            CmgmLog.fPositive("进游戏内容加载完成");

            await UniTask.CompletedTask;
        }
    }
}
