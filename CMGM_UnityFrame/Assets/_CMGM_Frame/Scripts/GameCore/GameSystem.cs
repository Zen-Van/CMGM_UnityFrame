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
    /// 游戏是否初始化完成的标记
    /// </summary>
    public static bool GameInitFinished = false;




    //回到主界面
    public static async UniTask GoToMainUI()
    {
        //清空运行时档案
        GameRuntimeData.Instance = null;

        //切回主界面
        UIManager.Instance.ClearPanel();
        await SceneManager.LoadSceneAsync("MainScene");
        UIManager.Instance.SetUICameraToMain();
        UIManager.Instance.ShowPanel<MainPanel>().Forget();
    }





    //每次切换场景时进行主动GC
}
