using CMGM.Core;
using CMGM.Data;
using Cysharp.Threading.Tasks;

namespace CMGM.Game
{
    /// <summary>
    /// 游戏层「进游戏」入口：在 Loading 进度条阶段加载 gameplay 所需内容（2.5 定位修订）。
    /// <para>Logo → 主界面 仅走框架 <see cref="CmgmFrameBoot"/>，不在此预加载角色表等大资源。</para>
    /// <para>进游戏 Loading 归属 Level 模块（ScenesManager），见 ARCHITECTURE 2.5c。</para>
    /// </summary>
    public static class GameBootstrap
    {
        /// <summary>
        /// 主界面点击「开始 / 读档」等进入游戏时调用（非 Logo 启动链）。
        /// <para>配表、关卡资源、Wwise Bank 等应在此（或 Loading 模块回调内）加载，见 ARCHITECTURE §8 资源加载分层。</para>
        /// </summary>
        public static async UniTask EnterGameplayAsync()
        {
            // TODO 2.5c：由 ScenesManager / LoadingPanel 展示进度并分段上报
            ConfigTableManager.Instance.LoadTable<RoleInfo>();
            // TODO 2.5c：预载关卡场景 / Addressables、Wwise Bank 等
            CmgmLog.fPositive("进游戏内容加载完成（配表等；Loading 见 Level 2.5c）");

            await UniTask.CompletedTask;
        }
    }
}
