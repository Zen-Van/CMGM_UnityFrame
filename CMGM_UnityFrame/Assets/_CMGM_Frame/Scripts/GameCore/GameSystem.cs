using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public static class GameSystem
{
    //角色的引用，在角色控制器初始化时注册，后注册的会覆盖先注册的
    public static PlayerController curPlayer;
    /// <summary>
    /// 游戏是否初始化完成的标记
    /// </summary>
    public static bool GameInitFinished = false;

    /// <summary>
    /// 退出游戏的方法
    /// </summary>
    public static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 回到主界面
    /// </summary>
    public static async UniTask GoToMainUI()
    {
        //清空运行时档案
        GameRuntimeData.Instance = null;

        //切回主界面
        UIManager.Instance.ClearPanel();
        await SceneManager.LoadSceneAsync("MainScene");
        UIManager.Instance.SetUICameraOverlap(Camera.main);
        UIManager.Instance.ShowPanel<MainPanel>().Forget();
    }

    /// <summary>
    /// 是否正在场景加载重
    /// </summary>
    public static bool isloading = false;




    //每次切换场景时进行主动GC
}
