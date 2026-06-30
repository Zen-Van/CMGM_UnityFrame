using CMGM.Core;
using CMGM.Data;
using CMGM.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CMGM.Scene
{
/// <summary>
/// 场景 / 流程临时宿主（2.5c 决策后）：
/// <para>· 场景加载原语已下沉至 <see cref="AddressablesResMgr.LoadSceneAsync"/>（Core）。</para>
/// <para>· <see cref="GoToMainScene"/> / <see cref="QuitGame"/> 属「流程控制」，待 **GameFlow系统** 接管。</para>
/// <para>· Scene 不再是独立 asmdef 模块；若未来需要 Additive / 流式 / 场景持久化，再扩为完整 Scene 模块。</para>
/// </summary>
public class ScenesManager : LazySingleton<ScenesManager>
{
    private ScenesManager() { }

    /// <summary>
    /// 回到主界面：主 Panel / 主场景名见 <see cref="CmgmFrameSettings"/>。
    /// <para><see cref="GoToMainScene"/> 逻辑已迁入 <see cref="CMGM.GameFlow.MainMenuState"/>（#7）。</para>
    /// </summary>
    public async UniTask GoToMainScene()
    {
        var settings = CmgmFrameSettings.Instance;

        //清空运行时档案
        ArchiveManager.Instance.ClearRuntimeData();
        //清空所有UI面板
        UIManager.Instance.ClearPanel();

        //切回主界面（Panel / 场景名见 Settings）
        await UIManager.Instance.ShowPanel(settings.MAIN_PANEL_NAME);
        await LoadSceneAsync(settings.MAIN_SCENE_NAME);
    }

    /// <summary>
    /// 退出游戏的方法。
    /// <para>TODO（GameFlow系统1.3）：迁入 GameFlow。</para>
    /// </summary>
    public void QuitGame() => CmgmApplication.Quit();

    /// <summary>
    /// 场景切换：调用 Core 资源原语加载，并处理 UI 摄像机叠加（UI 相关留在此处，不下沉 Core）。
    /// </summary>
    /// <param name="sceneName">场景名</param>
    /// <param name="loadSceneMode">加载模式</param>
    public async UniTask LoadSceneAsync(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        await AddressablesResMgr.Instance.LoadSceneAsync(sceneName, loadSceneMode);
        //每次加载完场景都要设置UI摄像机重叠
        UIManager.Instance.SetUICameraOverlap(Camera.main);
    }
}
}
