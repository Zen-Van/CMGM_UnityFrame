using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class ScenesManager : Singleton<ScenesManager>
{
    private ScenesManager() { }

    /// <summary>
    /// 回到主界面
    /// </summary>
    public async UniTask GoToMainScene()
    {
        //清空运行时档案
        GameArchiveManager.Instance.ClearRuntimeData();
        //清空所有UI面板
        UIManager.Instance.ClearPanel();

        //切回主界面
        await UIManager.Instance.ShowPanel<MainPanel>();
        await ScenesManager.Instance.LoadSceneAsync("MainScene", false);
    }

    /// <summary>
    /// 退出游戏的方法
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    /// <summary>
    /// 场景切换
    /// </summary>
    /// <param name="sceneName">场景名</param>
    /// <param name="loadSceneMode">加载模式</param>
    public async UniTask LoadSceneAsync(string sceneName, bool needLoadingPanel = true,LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        //加载场景
        await Addressables.LoadSceneAsync($"{Consts.Paths.HotScene}/{sceneName}.unity", loadSceneMode);
        //待补充进度提示相关代码
        if (needLoadingPanel)
        {
            //先独立显示UI界面
            UIManager.Instance.SetUICameraOverlap(null);
            //然后打开加载界面

        }
        //每次加载完场景都要设置UI摄像机重叠
        UIManager.Instance.SetUICameraOverlap(Camera.main);
    }
}
